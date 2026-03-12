export function imprimirComprobante(html) {
    const win = window.open('', '_blank', 'width=420,height=700');
    win.document.write(`
        <!DOCTYPE html><html><head>
        <title>Comprobante</title>
        <script src="https://cdnjs.cloudflare.com/ajax/libs/html2canvas/1.4.1/html2canvas.min.js"><\/script>
        <script src="https://cdnjs.cloudflare.com/ajax/libs/jspdf/2.5.1/jspdf.umd.min.js"><\/script>
        <style>
            * { margin:0; padding:0; box-sizing:border-box; }
            body { font-family: Arial, sans-serif; font-size: 12px; background:#fff; }
            #ticket {
                width: 350px;
                margin: 0 auto;
                padding: 16px;
                background: white;
            }
            h4 { text-align:center; font-size:15px; margin-bottom:2px; }
            .sub { text-align:center; color:#888; font-size:10px; margin-bottom:2px; }
            table { width:100%; border-collapse:collapse; margin-top:8px; }
            th { border-bottom:1px solid #ccc; padding:3px 4px; font-size:10px; color:#666; }
            td { padding:3px 4px; font-size:11px; }
            .total { font-weight:bold; font-size:13px; color:#0d6efd; }
            .badge { padding:2px 8px; border-radius:999px; font-size:10px; display:inline-block; margin:3px 0; }
            .success { background:#d1fae5; color:#065f46; }
            .danger  { background:#fee2e2; color:#991b1b; }
            .info-grid { display:grid; grid-template-columns:1fr 1fr; gap:6px; margin:8px 0; }
            .info-box { background:#f8f9fa; border-radius:6px; padding:6px; }
            .info-box .lbl { font-size:9px; color:#888; }
            .info-box .val { font-weight:bold; font-size:11px; }
            #btnDescargar {
                display:block; width:100%; padding:10px;
                background:#0d6efd; color:white; border:none;
                border-radius:8px; cursor:pointer; font-size:13px;
                font-weight:bold; margin-top:12px;
            }
        </style></head>
        <body>
            <div id="ticket">${html}</div>
            <div style="width:350px;margin:0 auto;">
                <button id="btnDescargar">🖨️ Guardar / Imprimir PDF</button>
            </div>
            <script>
                document.getElementById('btnDescargar').addEventListener('click', async () => {
                    const { jsPDF } = window.jspdf;
                    const ticket = document.getElementById('ticket');
                    const canvas = await html2canvas(ticket, { scale: 2, backgroundColor: '#ffffff' });
                    const imgData = canvas.toDataURL('image/png');
                    const pdfWidth = 80;
                    const pdfHeight = (canvas.height * pdfWidth) / canvas.width;
                    const pdf = new jsPDF({ unit: 'mm', format: [pdfWidth, pdfHeight] });
                    pdf.addImage(imgData, 'PNG', 0, 0, pdfWidth, pdfHeight);
                    pdf.save('comprobante.pdf');
                });
            <\/script>
        </body></html>`);
    win.document.close();
}