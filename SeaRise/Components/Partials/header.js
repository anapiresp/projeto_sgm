fetch("../../Partials/header.html")
    .then(response => response.text())
    .then(data => {
        document.getElementById("header-placeholder").innerHTML = data;


        if (!document.getElementById("header-css")) {
          const link = document.createElement("link");
          link.rel = "stylesheet";
          link.href = "../../Partials/header.css";
          link.id = "header-css";
          document.head.appendChild(link);
        }
    });
