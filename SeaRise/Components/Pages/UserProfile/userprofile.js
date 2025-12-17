document.addEventListener('DOMContentLoaded', () => {
    const changePasswordForm = document.getElementById('changePasswordForm');
    const changeNameForm = document.getElementById('changeNameForm');
    const logoutForm = document.getElementById('logoutForm');
    const message = document.getElementById('message');
    const nameMessage = document.getElementById('nameMessage');

    const currentPasswordInput = document.getElementById('currentPassword');
    const newPasswordInput = document.getElementById('newPassword');
    const confirmPasswordInput = document.getElementById('confirmPassword');

    // Logout form submission
    logoutForm?.addEventListener('submit', (e) => {
        e.preventDefault();
        if (confirm('Tens a certeza que queres sair?')) {
            localStorage.removeItem('user');
            window.location.href = '/Pages/SignUpLogin/signup.html';
        }
    });

    // Mudar password form submission
    changePasswordForm?.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (message) {
            message.textContent = '';
            message.style.color = '';
        }

        const currentPassword = currentPasswordInput?.value.trim() || '';
        const newPassword = newPasswordInput?.value.trim() || '';
        const confirmPassword = confirmPasswordInput?.value.trim() || '';

        // Validações básicas
        if (!currentPassword) {
            if (message) {
                message.textContent = 'Por favor, insere a tua password atual';
                message.style.color = 'red';
            }
            return;
        }

        if (!newPassword) {
            if (message) {
                message.textContent = 'Por favor, insere a nova password';
                message.style.color = 'red';
            }
            return;
        }

        if (newPassword.length < 8) {
            if (message) {
                message.textContent = 'A nova password tem de ter mais de 8 caracteres';
                message.style.color = 'red';
            }
            return;
        }

        if (newPassword !== confirmPassword) {
            if (message) {
                message.textContent = 'As passwords não coincidem';
                message.style.color = 'red';
            }
            return;
        }

        if (currentPassword === newPassword) {
            if (message) {
                message.textContent = 'A nova password tem de ser diferente da atual';
                message.style.color = 'red';
            }
            return;
        }

        // Obter o email do utilizador a partir do localStorage
        const userDataStr = localStorage.getItem('user');
        if (!userDataStr) {
            if (message) {
                message.textContent = 'Sessão expirada. Por favor, faz login novamente.';
                message.style.color = 'red';
            }
            setTimeout(() => {
                window.location.href = '/Pages/SignUpLogin/signup.html';
            }, 2000);
            return;
        }

        const userData = JSON.parse(userDataStr);
        const email = userData.Email || userData.email;

        if (!email) {
            if (message) {
                message.textContent = 'Email não encontrado. Por favor, faz login novamente.';
                message.style.color = 'red';
            }
            return;
        }

        const payload = {
            Email: email,
            CurrentPassword: currentPassword,
            NewPassword: newPassword
        };

        // Mostrar estado de carregamento
        const changePasswordBtn = document.getElementById('changePasswordBtn');
        if (changePasswordBtn) {
            changePasswordBtn.textContent = 'A alterar...';
            changePasswordBtn.disabled = true;
        }

        try {
            const res = await fetch('/api/auth/change-password', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (!res.ok) {
                const errorData = await res.json();
                throw new Error(errorData.message || 'Falha ao alterar password');
            }

            const data = await res.json();
            if (message) {
                message.textContent = (data.message || 'Password alterada com sucesso!');
                message.style.color = 'green';
            }

            // Limpar formulário
            if (currentPasswordInput) currentPasswordInput.value = '';
            if (newPasswordInput) newPasswordInput.value = '';
            if (confirmPasswordInput) confirmPasswordInput.value = '';

        } catch (err) {
            if (message) {
                message.textContent = 'Erro: ' + err.message;
                message.style.color = 'red';
            }
            console.error(err);
        } finally {
            if (changePasswordBtn) {
                changePasswordBtn.textContent = 'Mudar Palavra-passe';
                changePasswordBtn.disabled = false;
            }
        }
    });

    //Mudar nome de utilizador form submission
    changeNameForm?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const newNameInput = document.getElementById('newName');
        const newName = newNameInput?.value.trim() || '';

        // Validações básicas
        if (!newName) {
            if (nameMessage) {
                nameMessage.textContent = 'Por favor, insere o teu novo nome!';
                nameMessage.style.color = 'red';
            }
            return;
        }

        // Verifica se o novo nome tem pelo menos 1 espaço
        if (!newName.includes(' ')) {
            if (nameMessage) {
                nameMessage.textContent = 'O nome completo deve conter pelo menos um espaço!';
                nameMessage.style.color = 'red';
            }
            return;
        }

        // Obter o email do utilizador a partir do localStorage
        const userDataStr = localStorage.getItem('user');
        if (!userDataStr) {
            if (nameMessage) {
                nameMessage.textContent = 'Sessão expirada. Por favor, faz login novamente.';
                nameMessage.style.color = 'red';
            }
            setTimeout(() => {
                window.location.href = '/Pages/SignUpLogin/signup.html';
            }, 2000);
            return;
        }

        const userData = JSON.parse(userDataStr);
        const email = userData.Email || userData.email;
        if (!email) {
            if (nameMessage) {
                nameMessage.textContent = 'Email não encontrado. Por favor, faz login novamente.';
                nameMessage.style.color = 'red';
            }
            return;
        }

        // Verificar se o novo nome é diferente do atual
        if (userData.Username === newName) {
            if (nameMessage) {
                nameMessage.textContent = 'O novo nome tem de ser diferente do atual!';
                nameMessage.style.color = 'red';
            }
            return;
        }

        // Verificar se o novo nome não tem símbolos especiais
        const nameRegex = /^[a-zA-ZÀ-ÿ\s]+$/;
        if (!nameRegex.test(newName)) {
            if (nameMessage) {
                nameMessage.textContent = 'O nome não pode conter símbolos especiais ou números!';
                nameMessage.style.color = 'red';
            }
            return;
        }

        const payload = {
            Email: email,
            NewName: newName
        };

        const changeNameBtn = document.getElementById('changeNameBtn');
        if (changeNameBtn) {
            changeNameBtn.textContent = 'A alterar...';
            changeNameBtn.disabled = true;
        }

        try {
            const res = await fetch('/api/auth/change-username', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (!res.ok) {
                const errorData = await res.json();
                throw new Error(errorData.message || 'Falha ao alterar nome de utilizador');
            }
            const data = await res.json();
            if (nameMessage) {
                nameMessage.textContent = (data.message || 'Nome de utilizador alterado com sucesso!');
                nameMessage.style.color = 'green';
            }

            // Atualizar o nome no localStorage
            userData.Username = newName;
            localStorage.setItem('user', JSON.stringify(userData));
            // Limpar formulário
            if (newNameInput) newNameInput.value = '';
        } catch (err) {
            if (nameMessage) {
                nameMessage.textContent = 'Erro: ' + err.message;
                nameMessage.style.color = 'red';
            }
            console.error(err);
        } finally {
            if (changeNameBtn) {
                changeNameBtn.textContent = 'Mudar Nome de Utilizador';
                changeNameBtn.disabled = false;
            }
        }
    });

});
