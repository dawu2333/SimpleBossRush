const fs = require('node:fs');
const path = require('node:path');
const yamlPath = path.join(process.env.USERPROFILE, '.dsh', 'profiles', 'desktop', 'node_modules', 'js-yaml');
let YAML;
try { YAML = require(yamlPath); } catch (e) { YAML = require(path.join(process.env.USERPROFILE, '.dsh', 'profiles', 'desktop', 'node_modules', 'yaml')); }
const docPath = path.join(process.env.USERPROFILE, '.dsh', 'profiles', 'desktop', 'cordis.patch.yml');
const text = fs.readFileSync(docPath, 'utf8');
const doc = YAML.load ? YAML.load(text) : YAML.parse(text);
const entry = doc.find((e) => e.id === 'llm-pi-ai');
const alibaba = entry.config.providers.alibaba;
const lines = [];
lines.push('entries=' + doc.length);
lines.push('defaultContextWindow=' + String(alibaba.defaultContextWindow) + ' typeof=' + typeof alibaba.defaultContextWindow);
lines.push('models=' + alibaba.models.map((m) => m.id).join(','));
lines.push('alibabaKeys=' + Object.keys(alibaba).join(','));
lines.push('permissionPresets=' + Object.keys(doc.find((e) => e.id === 'permission').config.presets).join(','));
fs.writeFileSync(path.join(__dirname, 'yamlcheck.txt'), lines.join('\n'), 'utf8');