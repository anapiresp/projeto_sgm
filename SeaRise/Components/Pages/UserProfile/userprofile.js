document.addEventListener('DOMContentLoaded', () => {
    const changePasswordForm = document.getElementById('changePasswordForm');
    const logoutForm = document.getElementById('logoutForm');
    const message = document.getElementById('message');

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

    // Change password form submission
    changePasswordForm?.addEventListener('submit', async (e) => {
        e.preventDefault();
        if (message) {
            message.textContent = '';
            message.style.color = '';
        }

        const currentPassword = currentPasswordInput?.value.trim() || '';
        const newPassword = newPasswordInput?.value.trim() || '';
        const confirmPassword = confirmPasswordInput?.value.trim() || '';

        // Validations
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

        // Get user email from localStorage
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

        // Show loading state
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

            // Clear form
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
});
