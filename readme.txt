OVLÁDÁNÍ
wasd
ctrl + mouse scroll: změna citlivosti myši
mouse scroll: změna kostky
• aktuálně vybraná se vypíše na UI vlevo dole
left click: zničení zaměřené kostky
• čím déle se tlačítko drží, tím větší síla 
• na UI se zobrazí výsledek pokusu o rozbití kostky ve formátu: Try destroy: {hitStrength}/{neededStrength}
right click: položení vybrané kostky
space: jump
ESC: uvolní kurzor
F1: save game
• progres se vypisuje na debugu 
• po uložení se cesta zkopíruje do schránky
• mapa se automaticky načítá z uložených souborů
F3: hide UI

TERÉN
• na povrchu je tráva
• 5 kostek pod povrchem je hlína
• zbytek ke dnu je kámen
• na dně a na hranicích světa je nerozbitná skála

BUG
Z určitého úhlu a vzdálenosti kostka zešedivý (a ani za boha nemohu přijít na to, čím to je. Asi špatné normály?)

OPTIMALIZACE
• threading: zatím pouze ukládání. Ve vláknech by se mělo řešit: načítání, generování chunků.
• použít flat arrays. Momentálně jsou chunky uložené v 2D array, voxely v 3D array. 
• pokud je potřeba osvětlení: bake light maps

BUILD
https://ucnmuni-my.sharepoint.com/:f:/g/personal/410191_muni_cz/Egv4wu4LZltKkVGYn8l1Xq0B0N-PdVqq_skP6yAfB5ZCwA?e=i78htS