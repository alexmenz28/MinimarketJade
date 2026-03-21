function loadChartJs() {
    return new Promise((resolve, reject) => {
        if (window.Chart) {
            resolve(window.Chart);
            return;
        }

        const script = document.createElement('script');
        script.src = 'https://cdn.jsdelivr.net/npm/chart.js@4.4.1/dist/chart.umd.min.js';
        script.async = true;
        script.onload = () => resolve(window.Chart);
        script.onerror = reject;
        document.head.appendChild(script);
    });
}

const charts = {
    kpi1: null,
    kpi2: null,
    kpi5: null
};

function destroyChart(key) {
    if (charts[key]) {
        charts[key].destroy();
        charts[key] = null;
    }
}

export async function renderKpisProductos(data) {
    const Chart = await loadChartJs();

    const kpi1Labels = data?.kpi1?.labels ?? [];
    const kpi1Values = data?.kpi1?.values ?? [];
    const totalActivos = data?.kpi2?.totalActivos ?? 0;
    const totalCriticos = data?.kpi2?.totalCriticos ?? 0;
    const kpi2Labels = data?.kpi2?.labels ?? [];
    const kpi2Values = data?.kpi2?.values ?? [];
    const kpi2MinValues = data?.kpi2?.minValues ?? [];
    const kpi5Labels = data?.kpi5?.labels ?? [];
    const kpi5Values = data?.kpi5?.values ?? [];

    // KPI 1 (Más vendidos) - barras verticales compactas
    destroyChart('kpi1');
    const c1 = document.getElementById('kpi1Chart');
    if (c1 && kpi1Labels.length > 0) {
        charts.kpi1 = new Chart(c1, {
            type: 'bar',
            data: {
                labels: kpi1Labels,
                datasets: [{
                    label: 'Total vendido (unidades)',
                    data: kpi1Values,
                    backgroundColor: '#2563eb'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: { enabled: true }
                },
                scales: {
                    x: {
                        ticks: { maxRotation: 0, minRotation: 0, font: { size: 10 } },
                        grid: { display: false }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { precision: 0, font: { size: 10 } }
                    }
                }
            }
        });
    }

    // KPI 2 (Stock crítico) - donut críticos vs no críticos
    destroyChart('kpi2');
    const c2 = document.getElementById('kpi2Chart');
    if (c2 && totalActivos > 0) {
        const noCriticos = Math.max(0, totalActivos - totalCriticos);
        charts.kpi2 = new Chart(c2, {
            type: 'doughnut',
            data: {
                labels: ['Críticos', 'No críticos'],
                datasets: [{
                    label: 'Productos',
                    data: [totalCriticos, noCriticos],
                    backgroundColor: ['#ef4444', '#10b981'],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '62%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { boxWidth: 12, font: { size: 11 } }
                    },
                    tooltip: {
                        enabled: true,
                        callbacks: {
                            label: (ctx) => {
                                const val = ctx.raw ?? 0;
                                return `${ctx.label}: ${val}`;
                            }
                        }
                    }
                }
            }
        });
    }

    // KPI 5 (Menos vendidos) - barras horizontales compactas
    destroyChart('kpi5');
    const c5 = document.getElementById('kpi5Chart');
    if (c5 && kpi5Labels.length > 0) {
        charts.kpi5 = new Chart(c5, {
            type: 'bar',
            data: {
                labels: kpi5Labels,
                datasets: [{
                    label: 'Total vendido (unidades)',
                    data: kpi5Values,
                    backgroundColor: '#f59e0b'
                }]
            },
            options: {
                indexAxis: 'y',
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false }
                },
                scales: {
                    x: {
                        beginAtZero: true,
                        ticks: { precision: 0, font: { size: 10 } }
                    },
                    y: {
                        ticks: { autoSkip: false, font: { size: 10 } }
                    }
                }
            }
        });
    }
}

