const params = new URLSearchParams(window.location.search);
const jobTitle = params.get("title");

// Atualizar o título da página
if (jobTitle) {
    document.getElementById("job-title").textContent = jobTitle;
}
