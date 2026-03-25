const fs = require('fs');
const file = 'g:/source/repos/Scheduler/Scheduler/Scheduler.Client/src/app/components/messaging-admin/messaging-admin.component.scss';
let content = fs.readFileSync(file, 'utf8');

content = content.replace(/darken\(\$([a-zA-Z-]+),\s*([0-9.]+)%\)/g, (match, p1, p2) => {
    return `color-mix(in srgb, $${p1}, black ${p2}%)`;
});

content = content.replace(/rgba\(\$([a-zA-Z-]+),\s*([0-9.]+)\)/g, (match, p1, p2) => {
    return `color-mix(in srgb, $${p1} ${Math.round(parseFloat(p2) * 100)}%, transparent)`;
});

fs.writeFileSync(file, content);
console.log('Fixed SCSS color functions');
