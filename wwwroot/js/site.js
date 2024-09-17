document.addEventListener('DOMContentLoaded', function () {
    document.getElementById('maintenanceForm').addEventListener('submit', function (e) {
        var username = document.getElementById('username').value.trim();
        var serverAddress = document.getElementById('serverAddress').value.trim();
        var serverName = document.getElementById('serverName').value.trim();

        if (!username || !serverAddress || !serverName) {
            e.preventDefault(); 
            alert('Lütfen tüm alanları doldurun.');
        }
    });

    document.querySelector('.completed').addEventListener('click', function () {
        document.querySelectorAll('#maintenancePoints option').forEach(function (option) {
            option.selected = true;
        });
    });

    document.querySelector('.not-completed').addEventListener('click', function () {
        document.querySelectorAll('#maintenancePoints option').forEach(function (option) {
            option.selected = false;
        });
    });

    // Tablodaki hücreleri güncelleyen fonksiyon
    function updateTableCell(serverAddress, maintenancePointName, isCompleted) {
        const table = document.querySelector('.table tbody');
        const rows = table.querySelectorAll('tr');

        rows.forEach(row => {
            const serverCell = row.cells[0];
            const pointCell = row.cells[1];

            if (serverCell.textContent.trim() === serverAddress && pointCell.textContent.trim() === maintenancePointName) {
                const logCell = row.cells[row.cells.length - 1]; // Son hücre
                logCell.innerHTML = isCompleted ? '<span class="check-mark">&#10003;</span>' : '<span class="cross-mark">X</span>';
            }
        });
    }

    // Formun gönderim işlemi başarılı olduğunda tabloyu güncelleme
    document.querySelector('form[asp-action="UpdateMaintenanceStatus"]').addEventListener('submit', function (e) {
        e.preventDefault();

        const selectedPoints = Array.from(document.getElementById('maintenancePoints').selectedOptions).map(option => option.textContent.trim());
        const isCompleted = e.submitter.value === 'Yapıldı';
        const serverAddress = document.getElementById('serverAddress').value.trim();

        selectedPoints.forEach(point => {
            updateTableCell(serverAddress, point, isCompleted);
        });
    });
});







