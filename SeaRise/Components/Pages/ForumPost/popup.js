function openPopup() {
    document.getElementById("popup").style.display = "flex";
}

function closePopup() {
    document.getElementById("popup").style.display = "none";
}

function submitComment() {
    const text = document.getElementById("commentText").value;

    if (text.trim() === "") return;



    const commentsContainer = document.querySelector(".comments-title").nextElementSibling;

    const newComment = document.createElement("div");
    newComment.classList.add("comment");
    newComment.innerHTML = `
        <p class="comment-user">@maria</p>
        <p class="comment-text">${text}</p>
    `;

    commentsContainer.parentNode.appendChild(newComment);

    closePopup();
    document.getElementById("commentText").value = "";
}



document.getElementById("popup").addEventListener("click", function (e) {
    if (e.target === this) closePopup();
});



document.getElementById("deleteConfirm").addEventListener("click", function (e) {
    if (e.target === this) closeDeletePopup();
});







let commentToDelete = null;

function confirmDelete(btn) {
    commentToDelete = btn.parentElement;
    document.getElementById("deleteConfirm").style.display = "flex";
}

function closeDeletePopup() {
    document.getElementById("deleteConfirm").style.display = "none";
    commentToDelete = null;
}

function deleteComment() {
    if (commentToDelete) {
        commentToDelete.remove();
    }
    closeDeletePopup();
}
