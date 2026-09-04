window.leafletMap = {
    mapInstance: null,
    markersLayer: null,

    init: function (elementId) {
        if (this.mapInstance !== null) {
            this.mapInstance.off();
            this.mapInstance.remove();
            this.mapInstance = null;
            this.markersLayer = null;
        }

        this.mapInstance = L.map(elementId).setView([51.4938, 31.2953], 13);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19, attribution: '© OpenStreetMap'
        }).addTo(this.mapInstance);
    },

    loadData: function (locations, isAdmin, isSearchActive = false) {
        if (!this.mapInstance) return;

        if (this.markersLayer) {
            this.mapInstance.removeLayer(this.markersLayer);
        }

        this.markersLayer = L.featureGroup().addTo(this.mapInstance);

        locations.forEach(loc => {
            let tooltipContent = "";
            let tooltipOptions = { sticky: true };

            if (isAdmin) {
                let hasNotesIcon = loc.notes ? ' <span title="Є примітки">📝</span>' : '';
                tooltipContent = `<b>${loc.address}</b>${hasNotesIcon}<br/>Тип забудови: ${loc.buildingType}`;
            } else {
                let bizNames = "Заклад";
                if (loc.premises && loc.premises.length > 0) {
                    let namesArray = loc.premises.map(p => p.businessName ? p.businessName : p.businessCategory);
                    let uniqueNames = [...new Set(namesArray)];
                    bizNames = uniqueNames.join('<br/>');
                }

                if (isSearchActive) {
                    tooltipContent = `<div class="text-center text-primary fw-bold" style="font-size: 0.85rem; line-height: 1.1;">${bizNames}</div>`;
                    tooltipOptions = { permanent: true, direction: "center", opacity: 0.95 };
                } else {
                    tooltipContent = `<div class="text-center" style="line-height: 1.2;">
                                        <b style="font-size: 0.85rem;">${bizNames}</b><br/>
                                        <span class="text-muted" style="font-size: 0.75rem;">${loc.address}</span>
                                      </div>`;
                }
            }

            let popupContent = `
                <div style="min-width: 220px; max-width: 300px;">
                    <h6 class="mb-1 text-primary border-bottom pb-1">${loc.address}</h6>
            `;

            if (isAdmin) {
                popupContent += `
                    <div class="mb-2" style="font-size: 0.85rem;">
                        <strong>Тип:</strong> ${loc.buildingType}<br/>
                        <strong>Стан:</strong> ${loc.condition}
                    </div>
                `;

                if (loc.notes) {
                    popupContent += `<div class="alert alert-warning p-2 mb-2" style="font-size: 0.8rem;"><strong>Примітки:</strong><br/>${loc.notes}</div>`;
                }

                let statusSummary = {};
                if (loc.premises) {
                    loc.premises.forEach(p => {
                        statusSummary[p.status] = (statusSummary[p.status] || 0) + 1;
                    });
                }

                if (Object.keys(statusSummary).length > 0) {
                    popupContent += `<div class="mb-2" style="font-size: 0.85rem;"><strong>Приміщення:</strong><ul class="mb-0 ps-3">`;
                    for (const [status, count] of Object.entries(statusSummary)) {
                        popupContent += `<li>${status}: <strong>${count}</strong></li>`;
                    }
                    popupContent += `</ul></div>`;
                } else {
                    popupContent += `<div class="text-muted fst-italic mb-2" style="font-size: 0.8rem;">Немає зареєстрованих приміщень</div>`;
                }

                popupContent += `<a href="registry?highlight=${loc.id}" class="btn btn-sm btn-primary w-100" style="color: white !important;">Відкрити в реєстрі</a>`;
            } else {
                if (loc.premises && loc.premises.length > 0) {
                    popupContent += `<div class="mt-2 d-flex flex-column gap-2">`;
                    loc.premises.forEach(p => {
                        let catIcon = "bi-shop";
                        if (p.businessCategory === "Продукти / Супермаркет") catIcon = "bi-basket";
                        else if (p.businessCategory === "Кафе / Ресторан") catIcon = "bi-cup-hot";
                        else if (p.businessCategory === "СТО / Автомийка") catIcon = "bi-tools";
                        else if (p.businessCategory === "Аптека / Медицина") catIcon = "bi-capsule";

                        let bizName = p.businessName ? p.businessName : "Заклад / Послуги";
                        let bizDesc = p.businessDescription ? `<div class="mt-1 text-secondary" style="font-size: 0.8rem; line-height: 1.2;">${p.businessDescription}</div>` : "";
                        let bizCat = p.businessCategory ? `<div class="text-muted" style="font-size: 0.75rem;">${p.businessCategory}</div>` : "";

                        let scheduleHtml = '';
                        if (p.workingHours) {
                            try {
                                let schedule = JSON.parse(p.workingHours);
                                const daysMap = ["Нд", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"];
                                const todayStr = daysMap[new Date().getDay()];
                                let todaySchedule = schedule[todayStr];

                                let statusText = "🔴 Зачинено";
                                let statusColor = "text-danger";

                                if (todaySchedule && (todaySchedule.IsWorking || todaySchedule.isWorking)) {
                                    let openTime = (todaySchedule.Open || todaySchedule.open).substring(0, 5);
                                    let closeTime = (todaySchedule.Close || todaySchedule.close).substring(0, 5);
                                    
                                    let now = new Date();
                                    let currentTime = now.getHours().toString().padStart(2, '0') + ":" + now.getMinutes().toString().padStart(2, '0');

                                    if (currentTime >= openTime && currentTime < closeTime) {
                                        statusText = `🟢 Відчинено до ${closeTime}`;
                                        statusColor = "text-success";
                                    } else if (currentTime < openTime) {
                                        statusText = `🟠 Відчиниться о ${openTime}`;
                                        statusColor = "text-warning text-dark";
                                    }
                                }

                                let fullScheduleHtml = `<div class="mt-2 d-none" style="font-size: 0.75rem; border-left: 2px solid #dee2e6; padding-left: 8px;">`;
                                const orderedDays = ["Пн", "Вт", "Ср", "Чт", "Пт", "Сб", "Нд"];
                                orderedDays.forEach(d => {
                                    let s = schedule[d];
                                    let isToday = d === todayStr;
                                    let rowHtml = `<div class="d-flex justify-content-between mb-1">
                                                    <span class="${isToday ? 'fw-bold text-dark' : 'text-muted'}">${d}</span>`;
                                    if (s && (s.IsWorking || s.isWorking)) {
                                        let op = (s.Open || s.open).substring(0, 5);
                                        let cl = (s.Close || s.close).substring(0, 5);
                                        rowHtml += `<span class="${isToday ? 'fw-bold text-dark' : 'text-secondary'}">${op} - ${cl}</span>`;
                                    } else {
                                        rowHtml += `<span class="text-danger ${isToday ? 'fw-bold' : ''}" style="font-size: 0.7rem;">Вихідний</span>`;
                                    }
                                    rowHtml += `</div>`;
                                    fullScheduleHtml += rowHtml;
                                });
                                fullScheduleHtml += `</div>`;

                                scheduleHtml = `
                                    <div class="mt-2 pt-2 border-top border-light">
                                        <div class="d-flex justify-content-between align-items-center ${statusColor} fw-bold"
                                             style="font-size: 0.8rem; cursor: pointer; user-select: none;"
                                             onclick="this.nextElementSibling.classList.toggle('d-none'); let i = this.querySelector('i'); i.classList.toggle('bi-chevron-down'); i.classList.toggle('bi-chevron-up');">
                                            <span>${statusText}</span>
                                            <i class="bi bi-chevron-down text-muted"></i>
                                        </div>
                                        ${fullScheduleHtml}
                                    </div>
                                `;
                            } catch (e) {
                                console.error("Parse error schedule:", e);
                            }
                        }

                        popupContent += `
                            <div class="card shadow-sm border-0" style="background-color: #f8f9fa;">
                                <div class="card-body p-2">
                                    <div class="fw-bold text-dark d-flex align-items-center mb-1" style="font-size: 0.9rem;">
                                        <i class="bi ${catIcon} text-primary me-2 fs-5"></i> 
                                        ${bizName}
                                    </div>
                                    ${bizCat}
                                    ${bizDesc}
                                    ${scheduleHtml}
                                </div>
                            </div>
                        `;
                    });
                    popupContent += `</div>`;
                } else {
                    popupContent += `<div class="text-muted fst-italic mt-2" style="font-size: 0.8rem;">Інформація про заклади відсутня</div>`;
                }
            }

            popupContent += `</div>`;

            let iconEmoji = "📍";
            if (loc.buildingType.includes("Багатоповерхівка") || loc.buildingType.includes("Гуртожиток") || loc.buildingType.includes("Офісний")) iconEmoji = "🏢";
            else if (loc.buildingType.includes("Приватний")) iconEmoji = "🏠";
            else if (loc.buildingType.includes("Гараж")) iconEmoji = "🚗";
            else if (loc.buildingType.includes("Промисловий")) iconEmoji = "🏭";
            else if (loc.buildingType.includes("Комерційна") || loc.buildingType.includes("Громадська")) iconEmoji = "🏪";

            let customIcon = L.divIcon({
                className: 'custom-map-icon',
                html: `<div style="font-size: 18px; background: white; border-radius: 50%; width: 32px; height: 32px; display: flex; align-items: center; justify-content: center; border: 2px solid #007bff; box-shadow: 0 2px 5px rgba(0,0,0,0.3);">${iconEmoji}</div>`,
                iconSize: [32, 32],
                iconAnchor: [16, 16],
                popupAnchor: [0, -16]
            });

            if (loc.geoJson) {
                let geoJsonData = JSON.parse(loc.geoJson);
                let polyColor = loc.condition === 'В експлуатації' ? '#28a745' : '#007bff';
                if (loc.condition === 'Зруйновано (бойові дії)' || loc.condition === 'Аварійне') polyColor = '#dc3545';

                let polygon = L.geoJSON(geoJsonData, { style: { color: polyColor, weight: 2, fillOpacity: 0.4 } }).addTo(this.markersLayer);
                polygon.on('mouseover', function () { this.setStyle({ fillOpacity: 0.7 }); });
                polygon.on('mouseout', function () { this.setStyle({ fillOpacity: 0.4 }); });
                polygon.bindTooltip(tooltipContent, tooltipOptions);
                polygon.bindPopup(popupContent);
            }
            else if (loc.lat !== undefined && loc.lng !== undefined && loc.lat !== 0) {
                let marker = L.marker([loc.lat, loc.lng], { icon: customIcon }).addTo(this.markersLayer);
                marker.bindTooltip(tooltipContent, tooltipOptions);
                marker.bindPopup(popupContent);
            }
        });

        if (locations.length > 0) {
            this.mapInstance.fitBounds(this.markersLayer.getBounds(), { padding: [50, 50], maxZoom: 17 });
        }
    }
};