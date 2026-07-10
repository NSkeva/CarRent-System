(function () {
    function initFleetChat(root) {
        const askUrl = root.getAttribute('data-ask-url');
        const messages = root.querySelector('[data-chat-messages]');
        const input = root.querySelector('[data-chat-input]');
        const sendBtn = root.querySelector('[data-chat-send]');
        if (!askUrl || !messages || !input || !sendBtn) return;

        function appendMsg(text, role) {
            const div = document.createElement('div');
            div.className = 'chat-msg ' + (role === 'user' ? 'chat-msg-user' : 'chat-msg-bot');
            div.textContent = text;
            messages.appendChild(div);
            messages.scrollTop = messages.scrollHeight;
        }

        async function send() {
            const text = (input.value || '').trim();
            if (!text) return;
            appendMsg(text, 'user');
            input.value = '';
            sendBtn.disabled = true;
            try {
                const res = await fetch(askUrl, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    credentials: 'same-origin',
                    body: JSON.stringify({ message: text })
                });
                const contentType = res.headers.get('content-type') || '';
                if (!res.ok) {
                    appendMsg('Greška servera (' + res.status + '). Osvježite stranicu i pokušajte ponovo.', 'bot');
                    return;
                }
                if (!contentType.includes('application/json')) {
                    appendMsg('Neočekivan odgovor servera. Osvježite stranicu.', 'bot');
                    return;
                }
                const data = await res.json();
                appendMsg(data.reply || data.error || 'Greška.', 'bot');
            } catch {
                appendMsg('Greška mreže. Pokušajte ponovo.', 'bot');
            } finally {
                sendBtn.disabled = false;
                input.focus();
            }
        }

        sendBtn.addEventListener('click', send);
        input.addEventListener('keydown', (e) => {
            if (e.key === 'Enter') send();
        });

        document.querySelectorAll('[data-chat-suggestion]').forEach((btn) => {
            btn.addEventListener('click', () => {
                input.value = btn.getAttribute('data-chat-suggestion') || '';
                send();
            });
        });
    }

    document.querySelectorAll('[data-fleet-chat]').forEach(initFleetChat);
})();
