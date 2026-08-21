window.leafletMap = {
    init: function (elementId, locations) {
        var map = L.map(elementId).setView([51.4938, 31.2953], 13);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19, attribution: '© OpenStreetMap'
        }).addTo(map);

        locations.forEach(loc => {
            let tooltipContent = `<b>${loc.address}</b><br/>Тип забудови: ${loc.buildingType}`;

            let statusSummary = {};
            if (loc.premises) {
                loc.premises.forEach(p => {
                    statusSummary[p.status] = (statusSummary[p.status] || 0) + 1;
                });
            }

            let popupContent = `
                <div style="min-width: 200px;">
                    <h6 class="mb-1 text-primary border-bottom pb-1">${loc.address}</h6>
                    <div class="mb-2" style="font-size: 0.85rem;">
                        <strong>Тип:</strong> ${loc.buildingType}<br/>
                        <strong>Стан:</strong> ${loc.condition}
                    </div>
            `;

            if (Object.keys(statusSummary).length > 0) {
                popupContent += `<div class="mb-2" style="font-size: 0.85rem;"><strong>Приміщення:</strong><ul class="mb-0 ps-3">`;
                for (const [status, count] of Object.entries(statusSummary)) {
                    popupContent += `<li>${status}: <strong>${count}</strong></li>`;
                }
                popupContent += `</ul></div>`;
            } else {
                popupContent += `<div class="text-muted fst-italic mb-2" style="font-size: 0.8rem;">Немає зареєстрованих приміщень</div>`;
            }
            
            popupContent += `<a href="registry?highlight=${loc.id}" class="btn btn-sm btn-primary w-100">Відкрити в реєстрі</a>`;
            popupContent += `</div>`;

            if (loc.geoJson) {
                let geoJsonData = JSON.parse(loc.geoJson);
                let polyColor = loc.condition === 'В експлуатації' ? '#28a745' : '#007bff';
                let polygon = L.geoJSON(geoJsonData, { style: { color: polyColor, weight: 2, fillOpacity: 0.4 } }).addTo(map);
                polygon.on('mouseover', function () { this.setStyle({ fillOpacity: 0.7 }); });
                polygon.on('mouseout', function () { this.setStyle({ fillOpacity: 0.4 }); });
                polygon.bindTooltip(tooltipContent, { sticky: true });
                polygon.bindPopup(popupContent);
            }
            else if (loc.lat !== undefined && loc.lng !== undefined && loc.lat !== 0) {
                let marker = L.marker([loc.lat, loc.lng]).addTo(map);
                marker.bindTooltip(tooltipContent, { sticky: true });
                marker.bindPopup(popupContent);
            }
        });
    }
};