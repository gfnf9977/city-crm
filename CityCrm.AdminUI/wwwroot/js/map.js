window.leafletMap = {
    init: function (elementId, locations) {
        var map = L.map(elementId).setView([51.4938, 31.2953], 13);

        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '© OpenStreetMap'
        }).addTo(map);

        locations.forEach(loc => {
            // Перевіряємо, чи є координати, щоб не "ламати" Leaflet
            if (loc.lat !== undefined && loc.lng !== undefined) {
                L.marker([loc.lat, loc.lng]).addTo(map)
                    .bindPopup(`<b>${loc.address}</b><br/>Тип забудови: ${loc.buildingType}`);
            }
        });
    }
};