document.addEventListener('DOMContentLoaded', () => {
    console.log('[offer.js] DOMContentLoaded');
    // Toggle info popups on click (useful for touch devices)
    document.querySelectorAll('.info-wrapper').forEach(wrapper => {
        const btn = wrapper.querySelector('.info-btn');
        if (btn) {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();
                const isOpen = wrapper.classList.toggle('open');
                btn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
            });
        }
    });

    // Close popups when clicking outside
    document.addEventListener('click', () => {
        document.querySelectorAll('.info-wrapper.open').forEach(w => w.classList.remove('open'));
    });

    // Handle accept/dismiss inside popups
    // Modal behavior for CTA
    const modal = document.getElementById('offer-modal');
    const modalDialog = modal?.querySelector('.offer-modal');
    let openBtn = document.querySelector('.offer-cta');
    if (!openBtn) {
        // fallback: find any element that targets the modal via aria-controls
        openBtn = document.querySelector('[aria-controls="offer-modal"]');
    }
    console.log('[offer.js] modal:', !!modal, 'openBtn:', !!openBtn);
    const closeBtn = modal?.querySelector('.modal-close');
    const cancelBtn = modal?.querySelector('.modal-cancel');
    const acceptBtn = modal?.querySelector('.modal-accept');

    function openModal() {
        if (!modal) return;
        modal.removeAttribute('hidden');
        modal.setAttribute('aria-hidden', 'false');
        // set focus to modal for accessibility
        modalDialog?.focus();
    }
    function closeModal() {
        if (!modal) return;
        modal.setAttribute('aria-hidden', 'true');
        modal.setAttribute('hidden', '');
        openBtn?.focus();
    }

    // Primary listener
    openBtn?.addEventListener('click', (e) => { e.stopPropagation(); openModal(); });
    // Fallback: delegate clicks to document (useful if elements are injected later)
    if (!openBtn) {
        document.addEventListener('click', (e) => {
            const el = e.target.closest && e.target.closest('.offer-cta, [aria-controls="offer-modal"]');
            if (el) { e.stopPropagation(); openModal(); }
        });
    }
    closeBtn?.addEventListener('click', (e) => { e.stopPropagation(); closeModal(); });
    cancelBtn?.addEventListener('click', (e) => { e.stopPropagation(); closeModal(); });

    // Accept: show success inside modal (or call backend here)
    acceptBtn?.addEventListener('click', async (e) => {
        e.stopPropagation();
        acceptBtn.disabled = true;
        acceptBtn.textContent = 'A processar...';
        try {
            // Placeholder for server call
            await new Promise(r => setTimeout(r, 700));
            modalDialog.innerHTML = `<div class="success-inner"><div class="check">✓</div><div><strong>Oferta aceite!</strong><div>Voucher enviado para o seu e-mail.</div></div></div>`;
            setTimeout(() => closeModal(), 1800);
        } catch (err) {
            console.error(err);
            acceptBtn.disabled = false;
            acceptBtn.textContent = 'Aceitar oferta';
            alert('Erro ao aceitar a oferta. Tente novamente.');
        }
    });
});
