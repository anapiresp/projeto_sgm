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

    // Função para extrair mensagens de erro detalhadas
    async function getErrorMessage(response) {
        try {
            const data = await response.json();
            
            // Se for um dicionário de erros
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
            
            // Se for uma mensagem simples
            if (data.message) {
                return data.message;
            }
            
            return 'Erro desconhecido';
        } catch {
            return 'Erro ao processar resposta do servidor';
        }
    }

    // Função para validar password no frontend - apenas 8+ caracteres
    function getPasswordFeedback(password) {
        if (password.length < 8) {
            return 'Password tem de ter mais de 8 caracteres';
        }
        return null;
    }

    // Página do SIGN UP (login.html = criar conta)
    if (page === 'login') {
        const form = document.querySelector('.form-area');
        const typeSelect = document.querySelector('#type');
        const jobSelect = document.querySelector('#job');

        // Muda a exibição do campo de trabalho com base no tipo de utilizador
        typeSelect?.addEventListener('change', (e) => {
            if (e.target.value === 'Trabalho') {
                jobSelect.style.display = 'block';
            } else {
                jobSelect.style.display = 'none';
                jobSelect.value = ''; // Dá reset ao valor do trabalho
            }
        });

        form?.addEventListener('submit', async (e) => {
            e.preventDefault();
            const username = form.querySelector('input[name="username"]').value;
            const email = form.querySelector('input[type="email"]').value;
            const password = form.querySelector('input[type="password"]').value;
            const age = form.querySelector('input[type="number"]').value;
            const typeEl = form.querySelector('#type');
            const userType = typeEl ? typeEl.value : '';

            // Validação básica no frontend
            if (!username) {
                alert('Por favor, insere o teu primeiro e último nome!');
                return;
            }
            if (!email) {
                alert('Por favor, insere um email válido!');
                return;
            }
            if (!password) {
                alert('Por favor, insere uma password!');
                return;
            }
            if(!age){
                alert('Por favor, insere a tua idade!');
                return;
            }

            // Validar requisitos da password
            const feedback = getPasswordFeedback(password);
            if (feedback) {
                alert(feedback);
                return;
            }

            // Validar idade<18
            if (parseInt(age, 10) < 17) {
                alert('Deves ter pelo menos 18 anos para te registares!');
                return;
            }

            // Validar username com pelo menos 1 espaço
            if (!username.includes(' ')) {
                alert('Por favor, insere o teu primeiro e último nome (tem de ter um espaço pelo meio)!');
                return;
            }

            // Validar username sem símbolos especiais
            const nameRegex = /^[a-zA-ZÀ-ÿ\s]+$/;
            if (!nameRegex.test(username)) {
                alert('O nome não pode conter símbolos especiais ou números!');
                return;
            }

            // Validar userType
            if (!userType) {
                alert('Por favor, escolhe se trabalhas ou procuras trabalho!');
                return;
            }
            
            // Validar trabalho se tipo for Trabalho
            let job = '';
            if (userType === 'Trabalho') {
                job = (jobSelect && jobSelect.value) ? jobSelect.value : '';
                if (!job) {
                    alert('Escolhe o tipo de trabalho.');
                    return;
                }
            } else {
                job = 'Desempregado';
            }

            // Registar via AuthController
            const payload = { 
                Username: username, 
                Email: email, 
                Password: password, 
                UserType: userType,
                Age: parseInt(age, 10),
                Job: job
            };

            // Mostrar estado de carregamento
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
                    // Redireciona para a página de login após registo bem-sucedido
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

    // Página de LOGIN (signup.html = entrar na conta)
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

            // Mostrar estado de carregamento
            const btn = form.querySelector('button');
            const originalText = btn.textContent;
            btn.textContent = 'A entrar...';
            btn.disabled = true;

            try {
                const res = await postJson('/api/auth/login', { Email: email, Password: password });
                if (res.ok) {
                    const data = await res.json();
                    // Guardar dados do utilizador no localStorage
                    localStorage.setItem('user', JSON.stringify(data.user));
                    // Redireciona para a página principal após login bem-sucedido
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
