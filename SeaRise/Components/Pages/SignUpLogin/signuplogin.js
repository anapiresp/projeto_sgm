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

    // Signup page handler
    if (page === 'signup') {
        const form = document.querySelector('.form-area');
        form?.addEventListener('submit', async (e) => {
            e.preventDefault();
            const email = form.querySelector('input[type="email"]').value;
            const password = form.querySelector('input[type="password"]').value;
            const typeEl = form.querySelector('#type');
            const type = typeEl ? typeEl.value : '';

            // Create user via PUT (upsert)
            const payload = { username: email.split('@')[0], email, password, user_type: type };
            try {
                const res = await fetch('/api/user', {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(payload)
                });
                if (res.ok) {
                    // on success redirect to main
                    window.location.href = '/Pages/Main/main.html';
                } else {
                    const txt = await res.text();
                    alert('Erro ao registar: ' + txt);
                }
            } catch (err) {
                alert('Erro de rede: ' + err.message);
            }
        });
    }

    // Login page handler
    if (page === 'login') {
        const form = document.querySelector('.form-area');
        form?.addEventListener('submit', async (e) => {
            e.preventDefault();
            const email = form.querySelector('input[type="email"]').value;
            const password = form.querySelector('input[type="password"]').value;
            try {
                const res = await postJson('/api/user/login', { email, password });
                if (res.ok) {
                    // redirect to main
                    window.location.href = '/Pages/Main/main.html';
                } else if (res.status === 401) {
                    alert('Credenciais inválidas');
                } else {
                    const txt = await res.text();
                    alert('Erro: ' + txt);
                }
            } catch (err) {
                alert('Erro de rede: ' + err.message);
            }
        });
    }
});
