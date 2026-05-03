function attachRecaptchaV3(formId, siteKey) {
    const form = document.getElementById(formId);
    if (!form) return;

    // Remove any existing listener to prevent duplicates
    form.removeEventListener('submit', form._recaptchaHandler);

    form._recaptchaHandler = function (e) {
        const tokenInput = document.getElementById('recaptcha-token');
        
        // If we already have a token, let the form submit normally
        if (tokenInput && tokenInput.value) {
            return;
        }

        e.preventDefault();

        if (typeof grecaptcha === 'undefined') {
            console.error('reCAPTCHA is not loaded');
            // If it fails to load, allow submit so the server can handle the missing token gracefully
            if (typeof form.requestSubmit === 'function') {
                form.requestSubmit();
            } else {
                form.submit();
            }
            return;
        }

        grecaptcha.ready(function () {
            grecaptcha.execute(siteKey, { action: 'login' }).then(function (token) {
                if (tokenInput) {
                    tokenInput.value = token;
                }
                
                // Submit the form now that we have the token
                if (typeof form.requestSubmit === 'function') {
                    form.requestSubmit();
                } else {
                    form.submit();
                }
            }).catch(function(err) {
                console.error("reCAPTCHA execution error", err);
                if (typeof form.requestSubmit === 'function') form.requestSubmit();
                else form.submit();
            });
        });
    };

    form.addEventListener('submit', form._recaptchaHandler);
}
