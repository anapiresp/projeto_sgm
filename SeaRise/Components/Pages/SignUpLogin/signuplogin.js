document.addEventListener('DOMContentLoaded', () => {
    const page = document.body.dataset.page || '';

    async function postJson(url, payload) {
        const res = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        return res;
    }

    // Helper function to extract error messages from response
    async function getErrorMessage(response) {
        try {
            const data = await response.json();
            
            // If it's ASP.NET ModelState validation errors
            if (data.errors) {
                const errorMessages = [];
                for (const key in data.errors) {
                    const messages = data.errors[key];
                    if (Array.isArray(messages)) {
                        errorMessages.push(...messages);
                    } else if (typeof messages === 'string') {
                        errorMessages.push(messages);
                    }
                }
                return errorMessages.length > 0 ? errorMessages.join('\n') : 'Erro desconhecido';
            }
            
            // If it's a custom error message
            if (data.message) {
                return data.message;
            }
            
            return 'Erro desconhecido';
        } catch {
            return 'Erro ao processar resposta do servidor';
        }
    }

    // Validar password no frontend - apenas 8+ caracteres
    function validatePassword(password) {
        return password.length >= 8;
    }

    function getPasswordFeedback(password) {
        if (password.length < 8) {
            return 'Password tem de ter mais de 8 caracteres';
        }
        return null;
    }

    // Signup page handler (login.html = criar conta)
    if (page === 'login') {
        const form = document.querySelector('.form-area');

        form?.addEventListener('submit', async (e) => {
            e.preventDefault();
            const email = form.querySelector('input[type="email"]').value;
            const password = form.querySelector('input[type="password"]').value;
            const typeEl = form.querySelector('#type');
            const userType = typeEl ? typeEl.value : '';

            // Validação básica no frontend
            if (!email) {
                alert('Por favor, insere um email válido!');
                return;
            }

            if (!password) {
                alert('Por favor, insere uma password!');
                return;
            }

            // Validar requisitos da password
            const feedback = getPasswordFeedback(password);
            if (feedback) {
                alert(feedback);
                return;
            }

            if (!userType) {
                alert('Por favor, escolhe se traballas ou procuras trabalho!');
                return;
            }

            // Register via AuthController (C# PascalCase)
            const payload = { 
                Username: email.split('@')[0], 
                Email: email, 
                Password: password, 
                UserType: userType,
                Age: 18,  // Default to minimum age
                Job: ''
            };

            // Show loading state
            const btn = form.querySelector('button');
            const originalText = btn.textContent;
            btn.textContent = 'A registar...';
            btn.disabled = true;

            try {
                const res = await fetch('/api/auth/register', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                if (res.ok) {
                    // Redirect to login page without showing success message
                    window.location.href = '/Pages/SignUpLogin/signup.html';
                } else {
                    const errorMsg = await getErrorMessage(res);
                    alert('Erro ao registar:\n\n' + errorMsg);
                }
            } catch (err) {
                alert('Erro de rede:\n' + err.message + '\n\nVerifica a tua conexão!');
            } finally {
                btn.textContent = originalText;
                btn.disabled = false;
            }
        });
    }

    // Login page handler (signup.html = entrar na conta)
    if (page === 'signup') {
        const form = document.querySelector('.form-area');
        form?.addEventListener('submit', async (e) => {
            e.preventDefault();
            const email = form.querySelector('input[type="email"]').value;
            const password = form.querySelector('input[type="password"]').value;

            // Validação básica no frontend
            if (!email) {
                alert('Por favor, insere um email ou nome de utilizador!');
                return;
            }

            if (!password) {
                alert('Por favor, insere a tua password!');
                return;
            }

            // Show loading state
            const btn = form.querySelector('button');
            const originalText = btn.textContent;
            btn.textContent = 'A entrar...';
            btn.disabled = true;

            try {
                // C# PascalCase for LoginModel
                const res = await postJson('/api/auth/login', { UsernameOrEmail: email, Password: password });
                if (res.ok) {
                    const data = await res.json();
                    // Save user info to localStorage
                    localStorage.setItem('user', JSON.stringify(data.user));
                    // Redirect without showing message
                    window.location.href = '/Pages/Main/main.html';
                } else if (res.status === 401) {
                    const data = await res.json();
                    alert('Credenciais inválidas:\n\n' + (data.message || 'Email/username ou password incorretos'));
                } else {
                    const errorMsg = await getErrorMessage(res);
                    alert('Erro ao fazer login:\n\n' + errorMsg);
                }
            } catch (err) {
                alert('Erro de rede:\n' + err.message + '\n\nVerifica a tua conexão!');
            } finally {
                btn.textContent = originalText;
                btn.disabled = false;
            }
        });
    }
});
