document.addEventListener('DOMContentLoaded', () => {
    const dateInput = document.getElementById('appointment-date');

    if (dateInput) {
        const today = new Date();
        const year = today.getFullYear();
        const month = String(today.getMonth() + 1).padStart(2, '0');
        const day = String(today.getDate()).padStart(2, '0');

        const formattedDate = `${year}-${month}-${day}`;

        dateInput.value = formattedDate;

        dateInput.min = formattedDate;
    }
    const searchForm = document.getElementById('search-form');
    const resultsContainer = document.getElementById('results-container');

    searchForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const service = document.getElementById('service-type').value;
        const city = document.getElementById('city').value;
        const date = document.getElementById('appointment-date').value;

        resultsContainer.innerHTML = '<p>Szukam wolnych terminów...</p>';

        try {

            const data = [
                { clinicName: "Centrum Medyczne Kraków", doctor: "Dr. Jan Kowalski", slots: ["14:30", "15:00", "16:15"] },
                { clinicName: "Klinika Nowa", doctor: "Dr. Anna Nowak", slots: ["09:00", "11:30"] }
            ];

            renderResults(data);

        } catch (error) {
            console.error("Błąd pobierania danych:", error);
            resultsContainer.innerHTML = '<p>Wystąpił błąd podczas wyszukiwania.</p>';
        }
    });

    function renderResults(clinics) {
        resultsContainer.innerHTML = '';

        if (clinics.length === 0) {
            resultsContainer.innerHTML = '<p>Brak wolnych terminów w wybranym dniu.</p>';
            return;
        }

        clinics.forEach(clinic => {
            const card = document.createElement('div');
            card.classList.add('clinic-card');

            let htmlContent = `
                <h3>${clinic.clinicName}</h3>
                <p><strong>Lekarz:</strong> ${clinic.doctor}</p>
                <div class="slots-container" style="margin-top: 1rem;">
                    <h4>Dostępne godziny:</h4>
                    <div style="display: flex; gap: 0.5rem; flex-wrap: wrap; margin-top: 0.5rem;">
            `;

            clinic.slots.forEach(slot => {
                htmlContent += `<button class="slot-btn" data-time="${slot}">${slot}</button>`;
            });

            htmlContent += `</div></div>`;
            card.innerHTML = htmlContent;

            resultsContainer.appendChild(card);
        });

        attachSlotListeners();
    }

    function attachSlotListeners() {
        const slotButtons = document.querySelectorAll('.slot-btn');
        slotButtons.forEach(btn => {
            btn.addEventListener('click', (e) => {
                const time = e.target.getAttribute('data-time');
                alert(`Wybrano godzinę: ${time}. Nastąpi przekierowanie do logowania/potwierdzenia.`);
            });
        });
    }
});