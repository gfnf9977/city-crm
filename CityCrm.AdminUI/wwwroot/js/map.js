window.leafletMap = {
    mapInstance: null,
    buildingsCluster: null,
    issuesCluster: null,
    dotNetRef: null,
    pickingMode: false,
    moveTimeout: null,

    init: function (elementId, dotNetObj) {
        if (this.mapInstance !== null) {
            this.mapInstance.off();
            this.mapInstance.remove();
        }

        this.dotNetRef = dotNetObj;
        this.mapInstance = L.map(elementId).setView([51.4938, 31.2953], 13);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19, attribution: '© OpenStreetMap'
        }).addTo(this.mapInstance);

        this.buildingsCluster = L.markerClusterGroup({
            showCoverageOnHover: false,
            maxClusterRadius: 40
        }).addTo(this.mapInstance);

        this.issuesCluster = L.markerClusterGroup({
            showCoverageOnHover: false,
            maxClusterRadius: 40,
            iconCreateFunction: function(cluster) {
                return L.divIcon({ html: '<div style="background-color:rgba(220,53,69,0.8); color:white; border-radius:15px; text-align:center; line-height:30px; font-weight:bold;">' + cluster.getChildCount() + '</div>', className: 'issue-cluster', iconSize: L.point(30, 30) });
            }
        }).addTo(this.mapInstance);

        this.mapInstance.on('click', (e) => {
            if (this.pickingMode && this.dotNetRef) {
                this.disableIssuePicker();
                this.dotNetRef.invokeMethodAsync('OnMapClicked', e.latlng.lat, e.latlng.lng);
            }
        });

        this.mapInstance.on('moveend', () => {
            if (this.moveTimeout) clearTimeout(this.moveTimeout);

            this.moveTimeout = setTimeout(() => {
                if (this.dotNetRef) {
                    let bounds = this.mapInstance.getBounds();
                    let bboxStr = `${bounds.getWest()},${bounds.getSouth()},${bounds.getEast()},${bounds.getNorth()}`;
                    this.dotNetRef.invokeMethodAsync('OnMapMoved', bboxStr);
                }
            }, 400);
        });
    },

    enableIssuePicker: function () {
        this.pickingMode = true;
        document.getElementById('map').style.cursor = 'crosshair';
    },

    disableIssuePicker: function () {
        this.pickingMode = false;
        document.getElementById('map').style.cursor = 'grab';
    },

    loadData: function (locations, isAdmin, isSearchActive = false) {
        if (!this.mapInstance) return;

        this.buildingsCluster.clearLayers();
        let newLayers = [];

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

            let streetViewBtn = (loc.lat && loc.lng && loc.lat !== 0) 
                ? `<a href="https://www.google.com/maps/@?api=1&map_action=pano&viewpoint=${loc.lat},${loc.lng}" 
                      target="_blank" 
                      class="btn btn-sm btn-light border shadow-sm px-2 py-1 ms-2" 
                      title="Відкрити панораму (Street View)"
                      style="flex-shrink: 0;">
                      <i class="bi bi-eye-fill text-primary"></i>
                   </a>` 
                : '';

            let popupContent = `
                <div style="min-width: 220px; max-width: 300px;">
                    <div class="d-flex justify-content-between align-items-start border-bottom pb-2 mb-2">
                        <h6 class="mb-0 text-primary pe-1" style="line-height: 1.3;">${loc.address}</h6>
                        ${streetViewBtn}
                    </div>
            `;

            let badges = '';
            if (loc.hasShelter) badges += '<span class="badge bg-success me-1 mb-2 shadow-sm"><i class="bi bi-shield-shaded"></i> Укриття</span>';
            if (badges !== '') popupContent += `<div>${badges}</div>`;

            if (isAdmin) {
                popupContent += `
                    <div class="mb-2" style="font-size: 0.85rem;">
                        <strong>Тип:</strong> ${loc.buildingType}<br/>
                        <strong>Стан:</strong> ${loc.condition}
                    </div>
                `;
                if (loc.notes) popupContent += `<div class="alert alert-warning p-2 mb-2" style="font-size: 0.8rem;"><strong>Примітки:</strong><br/>${loc.notes}</div>`;

                let statusSummary = {};
                if (loc.premises) {
                    loc.premises.forEach(p => { statusSummary[p.status] = (statusSummary[p.status] || 0) + 1; });
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
                popupContent += `<a href="registry?highlight=${loc.id}" class="btn btn-sm btn-primary w-100 mb-1" style="color: white !important;">Відкрити в реєстрі</a>`;
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
                        let bizCat = p.businessCategory ? `<div class="text-muted d-inline-block" style="font-size: 0.75rem;">${p.businessCategory}</div>` : "";
                        let incBadge = p.isInclusive ? `<span class="badge bg-primary ms-2 shadow-sm" style="font-size: 0.65rem;"><i class="bi bi-person-wheelchair"></i> Безбар'єрно</span>` : "";

                        let rentBtnHtml = '';
                        if (!isAdmin && p.status === 'Вільне' && p.ownership === 'Комунальна' && p.type === 'Комерційна') {
                            if (p.prozorroLink) {
                                rentBtnHtml = `<div class="mt-2"><a href="${p.prozorroLink}" target="_blank" class="btn btn-sm btn-primary w-100 fw-bold" style="color: white !important;"><i class="bi bi-box-arrow-up-right"></i> Взяти участь на Prozorro</a></div>`;
                            } else {
                                rentBtnHtml = `<div class="mt-2"><a href="investors?premiseId=${p.id}" class="btn btn-sm btn-success w-100 fw-bold" style="color: white !important;"><i class="bi bi-hammer"></i> Ініціювати аукціон</a></div>`;
                            }
                        }

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

                                    if (openTime === "00:00" && closeTime === "23:59") {
                                        statusText = `🟢 Цілодобово 24/7`;
                                        statusColor = "text-success";
                                    } else if (currentTime >= openTime && currentTime < closeTime) {
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
                                        if (op === "00:00" && cl === "23:59") rowHtml += `<span class="${isToday ? 'fw-bold text-success' : 'text-success'}">Цілодобово</span>`;
                                        else rowHtml += `<span class="${isToday ? 'fw-bold text-dark' : 'text-secondary'}">${op} - ${cl}</span>`;
                                    } else rowHtml += `<span class="text-danger ${isToday ? 'fw-bold' : ''}" style="font-size: 0.7rem;">Вихідний</span>`;
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
                            } catch (e) { console.error("Parse error:", e); }
                        }

                        popupContent += `
                            <div class="card shadow-sm border-0" style="background-color: #f8f9fa;">
                                <div class="card-body p-2">
                                    <div class="fw-bold text-dark d-flex align-items-center mb-1" style="font-size: 0.9rem;">
                                        <i class="bi ${catIcon} text-primary me-2 fs-5"></i> ${bizName}
                                    </div>
                                    <div>${bizCat}${incBadge}</div>
                                    ${bizDesc}
                                    ${scheduleHtml}
                                    ${rentBtnHtml}
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
                iconSize: [32, 32], iconAnchor: [16, 16], popupAnchor: [0, -16]
            });

            if (loc.geoJson) {
                let geoJsonData = JSON.parse(loc.geoJson);
                let polyColor = loc.condition === 'В експлуатації' ? '#28a745' : '#007bff';
                if (loc.condition === 'Зруйновано (бойові дії)' || loc.condition === 'Аварійне') polyColor = '#dc3545';

                let polygon = L.geoJSON(geoJsonData, { style: { color: polyColor, weight: 2, fillOpacity: 0.4 } });
                polygon.on('mouseover', function () { this.setStyle({ fillOpacity: 0.7 }); });
                polygon.on('mouseout', function () { this.setStyle({ fillOpacity: 0.4 }); });
                polygon.bindTooltip(tooltipContent, tooltipOptions);
                polygon.bindPopup(popupContent);
                newLayers.push(polygon);
            }
            else if (loc.lat !== undefined && loc.lng !== undefined && loc.lat !== 0) {
                let marker = L.marker([loc.lat, loc.lng], { icon: customIcon });
                marker.bindTooltip(tooltipContent, tooltipOptions);
                marker.bindPopup(popupContent);
                newLayers.push(marker);
            }
        });

        if (newLayers.length > 0) {
            this.buildingsCluster.addLayers(newLayers);
            
            if (isSearchActive || !this.mapInstance.hasMoved) {
                this.mapInstance.fitBounds(this.buildingsCluster.getBounds(), { padding: [50, 50], maxZoom: 17 });
                this.mapInstance.hasMoved = true;
            }
        }
    },

    loadIssues: function (issues) {
        if (!this.mapInstance) return;
        this.issuesCluster.clearLayers();
        let newLayers = [];

        issues.forEach(issue => {
            let emoji = "⚠️";
            let color = "#dc3545";
            if (issue.status === "InProgress") { emoji = "🛠️"; color = "#ffc107"; }
            else if (issue.status === "Resolved") { emoji = "✅"; color = "#28a745"; }
            
            let issueIcon = L.divIcon({
                className: 'issue-map-icon',
                html: `<div style="font-size: 16px; background: white; border-radius: 50%; width: 28px; height: 28px; display: flex; align-items: center; justify-content: center; border: 2px solid ${color}; box-shadow: 0 2px 5px rgba(0,0,0,0.4);">${emoji}</div>`,
                iconSize: [28, 28], iconAnchor: [14, 14], popupAnchor: [0, -14]
            });

            let statusText = issue.status === "New" ? "Нова заявка" : issue.status === "InProgress" ? "В роботі" : "Вирішено";
            let popupContent = `
                <div style="min-width: 200px;">
                    <h6 class="mb-1 text-dark border-bottom pb-1"><i class="bi bi-exclamation-triangle-fill" style="color: ${color}"></i> ${issue.category}</h6>
                    <p class="small text-muted mb-2 fst-italic">"${issue.description}"</p>
                    <div class="badge" style="background-color: ${color}">${statusText}</div>
                    <div class="text-end mt-1 text-muted" style="font-size: 0.65rem;">${new Date(issue.createdAt).toLocaleDateString()}</div>
                </div>
            `;

            let marker = L.marker([issue.lat, issue.lng], { icon: issueIcon });
            marker.bindPopup(popupContent);
            newLayers.push(marker);
        });

        if (newLayers.length > 0) {
            this.issuesCluster.addLayers(newLayers);
        }
    },

    drawStreetLine: function (geoJsonStr) {
        if (!this.mapInstance) return;

        let geoJson = JSON.parse(geoJsonStr);
        
        let streetLayer = L.geoJSON(geoJson, {
            style: { color: '#8a2be2', weight: 6, opacity: 0.8 }
        }).addTo(this.mapInstance);

        this.mapInstance.fitBounds(streetLayer.getBounds(), { padding: [50, 50], maxZoom: 16 });
        
        setTimeout(() => {
            streetLayer.setStyle({ color: '#ff00ff', weight: 8 });
            setTimeout(() => streetLayer.setStyle({ color: '#8a2be2', weight: 6 }), 500);
        }, 500);
    },

    locateUser: function () {
        if (!this.mapInstance) return;
        this.mapInstance.locate({ setView: true, maxZoom: 17, enableHighAccuracy: true });
        this.mapInstance.once('locationfound', (e) => {
            let radius = e.accuracy / 2;
            L.circle(e.latlng, { radius: radius, color: '#007bff', fillOpacity: 0.2 }).addTo(this.mapInstance);
            L.circleMarker(e.latlng, { radius: 6, color: 'white', weight: 2, fillColor: '#007bff', fillOpacity: 1 }).addTo(this.mapInstance)
                .bindTooltip("Ви знаходитесь приблизно тут", { permanent: false, direction: "top" });
        });
        this.mapInstance.once('locationerror', (e) => {
            alert("Не вдалося визначити вашу локацію. Перевірте дозволи в браузері (GPS).");
        });
    }
};