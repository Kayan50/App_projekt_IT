document.addEventListener("DOMContentLoaded", () => {
    const dateInput = document.getElementById("appointment-date");

    if (dateInput) {
        const today = new Date();
        const year = today.getFullYear();
        const month = String(today.getMonth() + 1).padStart(2, "0");
        const day = String(today.getDate()).padStart(2, "0");
        const formattedDate = `${year}-${month}-${day}`;

        if (!dateInput.value) {
            dateInput.value = formattedDate;
        }
        dateInput.min = formattedDate;
    }

    
});