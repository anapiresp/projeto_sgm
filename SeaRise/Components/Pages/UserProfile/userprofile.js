document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('profileForm');
    const editBtn = document.getElementById('editBtn');
    const saveBtn = document.getElementById('saveBtn');
    const message = document.getElementById('message');

    const inputs = ['username', 'email', 'password'].map(id => document.getElementById(id));
    const hiddenId = document.getElementById('_id');

    function setDisabled(disabled) {
        inputs.forEach(i => i.disabled = disabled);
        hiddenId.disabled = disabled;
        saveBtn.disabled = disabled;
    }

    // Toggle edit mode
    editBtn.addEventListener('click', () => {
        const editable = inputs[0].disabled;
        setDisabled(!editable);
        if (!editable) {
            editBtn.textContent = 'Editar';
        } else {
            editBtn.textContent = 'Cancelar';
        }
    });

    // Load user data from API (if exists)
    async function loadUser() {
        try {
            const res = await fetch('/api/user');
            if (!res.ok) throw new Error('Sem dados');
            const data = await res.json();

            // _id from Mongo driver usually comes as { "$oid": "..." }
            let id = '';
            if (data._id) {
                if (typeof data._id === 'string') id = data._id;
                else id = data._id.$oid || data._id['$oid'] || '';
            }
            hiddenId.value = id;

            document.getElementById('username').value = data.username || data.name || '';
            document.getElementById('email').value = data.email || '';
            // For security, don't pre-fill password. Leave blank to keep the same.
            document.getElementById('password').value = '';
        } catch (err) {
            console.warn('Não foi possível obter utilizador:', err.message);
        }
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        message.textContent = '';
        const payload = {
            username: document.getElementById('username').value,
            email: document.getElementById('email').value
        };
        const pwd = document.getElementById('password').value;
        if (pwd && pwd.trim() !== '') payload.password = pwd;
        if (hiddenId.value) payload._id = hiddenId.value;

        try {
            const res = await fetch('/api/user', {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });
            if (!res.ok) throw new Error('Falha ao guardar');
            message.textContent = 'Dados guardados com sucesso.';
            setDisabled(true);
            editBtn.textContent = 'Editar';
        } catch (err) {
            message.textContent = 'Erro: não foi possível guardar os dados. Tente novamente.';
            console.error(err);
        }
    });

    // Inicializar
    setDisabled(true);
    loadUser();
});
