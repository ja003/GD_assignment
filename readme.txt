Ovládání
WASD
ctrl + mouse scroll: změna citlivosti myši
mouse scroll: změna kostičky
- aktuálně vybraná se vypíše vlevo dole
left click: zničení zaměřené kostky
- čím déle se tlačítko drží, tím větší síla 
- v debugu se zobrazí výsledek pokusu o rozbití kostky ve formátu: Try destroy: {hitStrength}/{neededStrength}
right click: položení vybrané kostky
space: jump
ESC: uvolní kurzor
F1: save game
- progres se vypisuje na debugu 
- po uložení se cesta zkopíruje do schránky
- mapa se automaticky načítá z uložených souborů
F3: hide UI

Terén
- na povrchu je tráva
- 5 kostiček pod povrchem je hlína
- zbytek ke dnu je kámen
- na dně a na hranicích světa je nerozbitná skála

Bug
Z určitého úhlu a vdálenosti kostička zešedivý...za boha nemohu přijít na to, čím to je. Asi špatné normály?

Optimalizace
- Threading: zatím pouze ukládání. 
Ve vláknech by se mělo řešit: načítání, generování chunků.

- use flat arrays. Momentálně jsou chunky uložené v Dictionary, voxely v 3D array. 

- pokud je potřeba osvětlení: bake light maps