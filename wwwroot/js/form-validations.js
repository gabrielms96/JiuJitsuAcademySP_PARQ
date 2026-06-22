(function (window, $) {
    'use strict';

    // ----- CPF -----

    function onlyDigits(value) {
        return (value || '').replace(/\D/g, '');
    }

    function formatCpf(value) {
        var digits = onlyDigits(value).slice(0, 11);
        return digits
            .replace(/(\d{3})(\d)/, '$1.$2')
            .replace(/(\d{3})(\d)/, '$1.$2')
            .replace(/(\d{3})(\d{1,2})$/, '$1-$2');
    }

    function isValidCpf(value) {
        var cpf = onlyDigits(value);
        if (cpf.length !== 11) return false;
        if (/^(\d)\1{10}$/.test(cpf)) return false;

        var sum = 0;
        for (var i = 0; i < 9; i++) sum += parseInt(cpf.charAt(i), 10) * (10 - i);
        var remainder = (sum * 10) % 11;
        if (remainder === 10 || remainder === 11) remainder = 0;
        if (remainder !== parseInt(cpf.charAt(9), 10)) return false;

        sum = 0;
        for (var j = 0; j < 10; j++) sum += parseInt(cpf.charAt(j), 10) * (11 - j);
        remainder = (sum * 10) % 11;
        if (remainder === 10 || remainder === 11) remainder = 0;
        return remainder === parseInt(cpf.charAt(10), 10);
    }

    function bindCpfMask(input) {
        input.addEventListener('input', function () {
            var start = input.selectionStart;
            var before = input.value.length;
            input.value = formatCpf(input.value);
            var after = input.value.length;
            var pos = Math.max(0, start + (after - before));
            input.setSelectionRange(pos, pos);
        });

        input.addEventListener('blur', function () {
            if (onlyDigits(input.value).length === 11 && !isValidCpf(input.value)) {
                input.setCustomValidity('Informe um CPF válido.');
            } else {
                input.setCustomValidity('');
            }
        });
    }

    // ----- CEP (ViaCEP — dados dos Correios, gratuito) -----

    function formatCep(value) {
        var digits = onlyDigits(value).slice(0, 8);
        if (digits.length <= 5) return digits;
        return digits.slice(0, 5) + '-' + digits.slice(5);
    }

    var cepCache = Object.create(null);
    var cepRequestId = 0;

    function lookupCep(cep, onSuccess, onError) {
        var digits = onlyDigits(cep);
        if (digits.length !== 8) return;

        if (cepCache[digits]) {
            onSuccess(cepCache[digits]);
            return;
        }

        var requestId = ++cepRequestId;
        fetch('https://viacep.com.br/ws/' + digits + '/json/', {
            method: 'GET',
            headers: { Accept: 'application/json' }
        })
            .then(function (response) {
                if (!response.ok) throw new Error('network');
                return response.json();
            })
            .then(function (data) {
                if (requestId !== cepRequestId) return;
                if (data.erro) {
                    onError('CEP não encontrado.');
                    return;
                }
                cepCache[digits] = data;
                onSuccess(data);
            })
            .catch(function () {
                if (requestId !== cepRequestId) return;
                onError('Não foi possível consultar o CEP. Verifique sua conexão.');
            });
    }

    function initAddressByCep(options) {
        var cepInput = document.getElementById(options.cepId);
        var addressBlock = document.getElementById(options.addressBlockId);
        var statusEl = document.getElementById(options.statusId);
        var fields = options.fields;

        if (!cepInput || !addressBlock) return;

        function setStatus(message, type) {
            if (!statusEl) return;
            statusEl.textContent = message || '';
            statusEl.className = 'cep-status' + (type ? ' cep-status--' + type : '');
        }

        function clearAddressFields() {
            fields.logradouro.value = '';
            fields.bairro.value = '';
            fields.cidade.value = '';
            fields.estado.value = '';
            cepInput.dataset.cepValid = 'false';
        }

        function showAddressBlock(show) {
            addressBlock.classList.toggle('d-none', !show);
            addressBlock.setAttribute('aria-hidden', show ? 'false' : 'true');
        }

        function fillAddress(data) {
            fields.logradouro.value = data.logradouro || '';
            fields.bairro.value = data.bairro || '';
            fields.cidade.value = data.localidade || '';
            fields.estado.value = data.uf || '';
            cepInput.dataset.cepValid = 'true';
            showAddressBlock(true);
            setStatus('Endereço encontrado. Informe número e complemento.', 'success');
            fields.numero.focus();
        }

        function consultCep() {
            var digits = onlyDigits(cepInput.value);
            if (digits.length !== 8) {
                cepInput.dataset.cepValid = 'false';
                showAddressBlock(false);
                clearAddressFields();
                setStatus('');
                return;
            }

            setStatus('Consultando CEP...', 'loading');
            cepInput.dataset.cepValid = 'false';
            showAddressBlock(false);
            clearAddressFields();

            lookupCep(
                digits,
                function (data) { fillAddress(data); },
                function (msg) {
                    setStatus(msg, 'error');
                    showAddressBlock(false);
                }
            );
        }

        cepInput.addEventListener('input', function () {
            var start = cepInput.selectionStart;
            var before = cepInput.value.length;
            cepInput.value = formatCep(cepInput.value);
            var after = cepInput.value.length;
            var pos = Math.max(0, start + (after - before));
            cepInput.setSelectionRange(pos, pos);

            if (onlyDigits(cepInput.value).length < 8) {
                cepInput.dataset.cepValid = 'false';
                showAddressBlock(false);
                clearAddressFields();
                setStatus('');
            }
        });

        cepInput.addEventListener('blur', consultCep);

        // Se o formulário voltou com erro do servidor, reexibe endereço preenchido
        if (onlyDigits(cepInput.value).length === 8 && fields.logradouro.value.trim()) {
            cepInput.dataset.cepValid = 'true';
            showAddressBlock(true);
            setStatus('Confira o endereço e informe número e complemento.', 'success');
        }
    }

    // ----- jQuery Validation (integração com unobtrusive já carregado) -----

    var rulesRegistered = false;

    function registerJqueryRules() {
        if (!$ || !$.validator || rulesRegistered) return;

        $.validator.addMethod('cpfBR', function (value, element) {
            return this.optional(element) || isValidCpf(value);
        }, 'Informe um CPF válido.');

        $.validator.addMethod('cepValidado', function (_value, element) {
            return element.dataset.cepValid === 'true';
        });

        if ($.validator.unobtrusive) {
            // addBool usa o valor do atributo como mensagem — "true" virava texto de erro.
            $.validator.unobtrusive.adapters.add('cpfbr', function (options) {
                options.rules.cpfBR = true;
                options.messages.cpfBR = options.message || 'Informe um CPF válido.';
            });
            $.validator.unobtrusive.adapters.add('cepvalidado', function (options) {
                options.rules.cepValidado = true;
                options.messages.cepValidado = options.message || 'Informe um CEP válido e aguarde a consulta.';
            });
        }

        rulesRegistered = true;

        var form = $('#parqForm');
        if (form.length && $.validator.unobtrusive) {
            form.removeData('validator');
            form.removeData('unobtrusiveValidation');
            $.validator.unobtrusive.parse(form);
        }
    }

    function init() {
        document.querySelectorAll('[data-mask="cpf"]').forEach(bindCpfMask);

        initAddressByCep({
            cepId: 'Form_Cep',
            addressBlockId: 'addressFields',
            statusId: 'cepStatus',
            fields: {
                logradouro: document.getElementById('Form_Logradouro'),
                numero: document.getElementById('Form_Numero'),
                complemento: document.getElementById('Form_Complemento'),
                bairro: document.getElementById('Form_Bairro'),
                cidade: document.getElementById('Form_Cidade'),
                estado: document.getElementById('Form_Estado')
            }
        });

        registerJqueryRules();

        var form = document.getElementById('parqForm');
        if (form) {
            form.addEventListener('submit', function () {
                document.querySelectorAll('[data-mask="cpf"]').forEach(function (input) {
                    input.setCustomValidity(
                        input.value && !isValidCpf(input.value) ? 'Informe um CPF válido.' : ''
                    );
                });
            });
        }
    }

    window.FormValidations = {
        isValidCpf: isValidCpf,
        formatCpf: formatCpf,
        formatCep: formatCep
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})(window, window.jQuery);
