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

    loadData: function (locations, isAdmin) {
        if (!this.mapInstance) return;

        if (this.markersLayer) {
            this.mapInstance.removeLayer(this.markersLayer);
        }

        this.markersLayer = L.featureGroup().addTo(this.mapInstance);

        locations.forEach(loc => {
            let hasNotesIcon = loc.notes ? ' <span title="Є примітки">📝</span>' : '';
            let tooltipContent = `<b>${loc.address}</b>${hasNotesIcon}<br/>Тип забудови: ${loc.buildingType}`;

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

            if (loc.notes) {
                popupContent += `<div class="alert alert-warning p-2 mb-2" style="font-size: 0.8rem;"><strong>Примітки:</strong><br/>${loc.notes}</div>`;
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

            if (isAdmin) {
                popupContent += `<a href="registry?highlight=${loc.id}" class="btn btn-sm btn-primary w-100" style="color: white !important;">Відкрити в реєстрі</a>`;
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
                polygon.bindTooltip(tooltipContent, { sticky: true });
                polygon.bindPopup(popupContent);
            }
            else if (loc.lat !== undefined && loc.lng !== undefined && loc.lat !== 0) {
                let marker = L.marker([loc.lat, loc.lng], { icon: customIcon }).addTo(this.markersLayer);
                marker.bindTooltip(tooltipContent, { sticky: true });
                marker.bindPopup(popupContent);
            }
        });

        if (locations.length > 0) {
            this.mapInstance.fitBounds(this.markersLayer.getBounds(), { padding: [50, 50], maxZoom: 17 });
        }
    }
};