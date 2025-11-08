    document.addEventListener("DOMContentLoaded", function () {
        const toggleBtn = document.getElementById("toggleFilter");
        const filterPanel = document.getElementById("filterPanel");

        toggleBtn.addEventListener("click", function () {
            if (filterPanel.style.display === "none" || filterPanel.style.display === "") {
                filterPanel.style.display = "block";
            } else {
                filterPanel.style.display = "none";
            }
        });
    });


