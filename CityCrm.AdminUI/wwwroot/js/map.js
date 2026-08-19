window.leafletMap = {
    init: function (elementId, locations) {
        var map = L.map(elementId).setView([51.4938, 31.2953], 13);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '© OpenStreetMap'
        }).addTo(map);

        locations.forEach(loc => {
            if (loc.geoJson) {
                let geoJsonData = JSON.parse(loc.geoJson);
                let polyColor = loc.condition === 'В експлуатації' ? '#28a745' : '#007bff';

                let polygon = L.geoJSON(geoJsonData, {
                    style: {
                        color: polyColor,
                        weight: 2,
                        fillOpacity: 0.4
                    }
                }).addTo(map);

                polygon.on('mouseover', function () { this.setStyle({ fillOpacity: 0.7 }); });
                polygon.on('mouseout', function () { this.setStyle({ fillOpacity: 0.4 }); });

                polygon.bindTooltip(`<b>${loc.address}</b><br/>Тип забудови: ${loc.buildingType}`, { sticky: true });
                polygon.bindPopup(`<b>${loc.address}</b><br/>Тип забудови: ${loc.buildingType}`);
            }
            else if (loc.lat !== undefined && loc.lng !== undefined && loc.lat !== 0) {
                let marker = L.marker([loc.lat, loc.lng]).addTo(map);

                marker.bindTooltip(`<b>${loc.address}</b><br/>Тип забудови: ${loc.buildingType}`, { sticky: true });
                marker.bindPopup(`<b>${loc.address}</b><br/>Тип забудови: ${loc.buildingType}`);
            }
        });
    }
};