document.addEventListener("DOMContentLoaded", () => {
  const dateInput = document.getElementById("appointment-date");

  if (dateInput) {
    const today = new Date();
    const year = today.getFullYear();
    const month = String(today.getMonth() + 1).padStart(2, "0");
    const day = String(today.getDate()).padStart(2, "0");
    const formattedDate = `${year}-${month}-${day}`;

    dateInput.value = formattedDate;
    dateInput.min = formattedDate;
  }

  const citySelect = document.getElementById("city-select");
  const serviceSelect = document.getElementById("service-select");

  async function loadCities() {
    if (!citySelect) return;
    try {
      const response = await fetch("/api/SearchApi/cities");
      if (!response.ok) throw new Error();
      const cities = await response.json();
      citySelect.innerHTML = '<option value="">Wybierz miasto</option>';
      cities.forEach((city) => {
        const option = document.createElement("option");
        option.value = city.id;
        option.textContent = city.name;
        citySelect.appendChild(option);
      });
    } catch (error) {
      citySelect.innerHTML = '<option value="">Błąd serwera</option>';
    }
  }

  async function loadServices() {
    if (!serviceSelect) return;
    try {
      const response = await fetch("/api/SearchApi/services");
      if (!response.ok) throw new Error();
      const services = await response.json();
      serviceSelect.innerHTML = '<option value="">Wybierz usługę</option>';
      services.forEach((service) => {
        const option = document.createElement("option");
        option.value = service.id;
        option.textContent = service.name;
        if (service.isNFZ) {
          option.dataset.nfz = true;
        }
        serviceSelect.appendChild(option);
      });
    } catch (error) {
      serviceSelect.innerHTML = '<option value="">Błąd serwera</option>';
    }
  }

  loadCities();
  loadServices();

  const searchForm = document.getElementById("search-form");
  const resultsContainer = document.getElementById("results-container");

  if (searchForm) {
    searchForm.addEventListener("submit", async (e) => {
      e.preventDefault();

      const cityId = document.getElementById("city-select").value;
      const serviceId = document.getElementById("service-select").value;
      const date = dateInput.value;

      const visitTypeRadio = document.querySelector(
        'input[name="visitType"]:checked',
      );
      const isNfz =
        visitTypeRadio && visitTypeRadio.value === "NFZ" ? "true" : "false";

      resultsContainer.innerHTML =
        '<div style="grid-column: 1 / -1; text-align: center;"><h3>Szukam wolnych terminów...</h3></div>';

      try {
        const url = `/api/SearchApi/slots?cityId=${cityId}&serviceId=${serviceId}&date=${date}&isNfz=${isNfz}`;
        const response = await fetch(url);

        if (!response.ok) throw new Error("Błąd serwera podczas wyszukiwania");

        const clinics = await response.json();
        resultsContainer.innerHTML = "";

        if (clinics.length === 0) {
          resultsContainer.innerHTML = `
            <div style="grid-column: 1 / -1; text-align: center; background: white; padding: 2rem; border-radius: 12px; box-shadow: var(--card-shadow);">
              <h3 style="color: var(--brand-bg); margin-bottom: 1rem;">Brak wolnych terminów na ten dzień 😔</h3>
              <p>Spróbuj zmienić datę lub kryteria wyszukiwania.</p>
              <div style="margin-top: 1.5rem; display: flex; justify-content: center; gap: 1rem;">
                <button type="button" id="prev-day-btn" class="btn-primary" style="background-color: #64748b;">&larr; Wczoraj</button>
                <button type="button" id="next-day-btn" class="btn-primary">Jutro &rarr;</button>
              </div>
            </div>
          `;
          setupDateButtons(dateInput);
          return;
        }

        clinics.forEach((clinic) => {
          const card = document.createElement("div");
          card.className = "clinic-card";
          const slotsHtml = clinic.availableSlots
            .map(
              (slot) =>
                `<button class="slot-btn" data-slot-id="${slot.slotId}">${slot.time}</button>`,
            )
            .join("");

          card.innerHTML = `
            <h3 style="color: var(--brand-bg); margin-bottom: 0.5rem;">${clinic.clinicName}</h3>
            <p style="color: #64748b; font-size: 0.95rem; margin-bottom: 1.5rem;">
              <strong>Lekarz:</strong> ${clinic.doctorName}
            </p>
            <div style="display: flex; gap: 0.5rem; flex-wrap: wrap;">
              ${slotsHtml}
            </div>
          `;
          resultsContainer.appendChild(card);
        });
      } catch (error) {
        console.error("Błąd wyszukiwania:", error);
        resultsContainer.innerHTML =
          '<div style="grid-column: 1 / -1; text-align: center; color: #e11d48;"><h3>Wystąpił błąd komunikacji z serwerem.</h3></div>';
      }
    });
  }

  function setupDateButtons(dateInput) {
    const prevBtn = document.getElementById("prev-day-btn");
    const nextBtn = document.getElementById("next-day-btn");

    if (prevBtn) {
      prevBtn.addEventListener("click", () =>
        changeDateAndSubmit(-1, dateInput),
      );
    }
    if (nextBtn) {
      nextBtn.addEventListener("click", () =>
        changeDateAndSubmit(1, dateInput),
      );
    }
  }

  function changeDateAndSubmit(daysToAdd, dateInput) {
    const currentDate = new Date(dateInput.value);
    currentDate.setDate(currentDate.getDate() + daysToAdd);
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    if (currentDate < today) return;

    const year = currentDate.getFullYear();
    const month = String(currentDate.getMonth() + 1).padStart(2, "0");
    const day = String(currentDate.getDate()).padStart(2, "0");

    dateInput.value = `${year}-${month}-${day}`;
    searchForm.dispatchEvent(new Event("submit"));
  }
});
