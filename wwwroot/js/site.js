// Progressive enhancement: submitting the GET form also works without JavaScript.
(() => {
    const input = document.getElementById('game-search');
    if (!input) return;
    const entries = [...document.querySelectorAll('[data-game-name]')];
    const groups = [...document.querySelectorAll('[data-letter-group]')];
    const count = document.getElementById('game-result-count');
    const empty = document.getElementById('game-empty');
    input.addEventListener('input', () => {
        const query = input.value.trim().toLocaleLowerCase();
        let visible = 0;
        entries.forEach(entry => {
            entry.hidden = !entry.dataset.gameName.includes(query);
            if (!entry.hidden) visible++;
        });
        groups.forEach(group => {
            group.hidden = ![...group.querySelectorAll('[data-game-name]')].some(entry => !entry.hidden);
            const link = document.querySelector(`[data-letter="${CSS.escape(group.dataset.letterGroup)}"]`);
            if (link) link.hidden = group.hidden;
        });
        document.querySelectorAll('.category-tabs a, .offers-toggle').forEach(link => {
            const target = new URL(link.href);
            if (input.value.trim()) target.searchParams.set('q', input.value.trim());
            else target.searchParams.delete('q');
            link.href = target.toString();
        });
        count.textContent = visible;
        empty.hidden = visible !== 0;
    });
})();
