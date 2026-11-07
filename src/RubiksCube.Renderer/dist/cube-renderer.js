import * as THREE from 'three';
import { OrbitControls } from 'three/addons/controls/OrbitControls.js';
const COLOR_HEX = {
    white: 0xffffff,
    yellow: 0xf3d21b,
    green: 0x18d64b,
    blue: 0x3987ff,
    red: 0xc72e4a,
    orange: 0xff8a1e,
};
const STICKER_MAP = {};
for (let row = 0; row < 3; row++) {
    for (let col = 0; col < 3; col++) {
        STICKER_MAP[`0,${row},${col}`] = { x: col - 1, y: 1, z: 1 - row, nx: 0, ny: 1, nz: 0 };
    }
}
for (let row = 0; row < 3; row++) {
    for (let col = 0; col < 3; col++) {
        STICKER_MAP[`1,${row},${col}`] = { x: 1, y: 1 - row, z: col - 1, nx: 1, ny: 0, nz: 0 };
    }
}
for (let row = 0; row < 3; row++) {
    for (let col = 0; col < 3; col++) {
        STICKER_MAP[`2,${row},${col}`] = { x: col - 1, y: 1 - row, z: 1, nx: 0, ny: 0, nz: 1 };
    }
}
for (let row = 0; row < 3; row++) {
    for (let col = 0; col < 3; col++) {
        STICKER_MAP[`3,${row},${col}`] = { x: col - 1, y: -1, z: row - 1, nx: 0, ny: -1, nz: 0 };
    }
}
for (let row = 0; row < 3; row++) {
    for (let col = 0; col < 3; col++) {
        STICKER_MAP[`4,${row},${col}`] = { x: -1, y: 1 - row, z: 1 - col, nx: -1, ny: 0, nz: 0 };
    }
}
for (let row = 0; row < 3; row++) {
    for (let col = 0; col < 3; col++) {
        STICKER_MAP[`5,${row},${col}`] = { x: 1 - col, y: 1 - row, z: -1, nx: 0, ny: 0, nz: -1 };
    }
}
const FACE_NAMES = ['U', 'R', 'F', 'D', 'L', 'B'];
function parseMove(move) {
    const m = move.match(/^([URFLDB])(2|')?$/);
    if (!m)
        return null;
    const face = m[1];
    const modifier = m[2];
    const faceIndex = FACE_NAMES.indexOf(face);
    const specs = [
        { axis: 'x', layer: 1, angle: -Math.PI / 2 },
        { axis: 'x', layer: -1, angle: Math.PI / 2 },
        { axis: 'y', layer: 1, angle: -Math.PI / 2 },
        { axis: 'y', layer: -1, angle: Math.PI / 2 },
        { axis: 'z', layer: 1, angle: -Math.PI / 2 },
        { axis: 'z', layer: -1, angle: -Math.PI / 2 },
    ];
    const spec = specs[faceIndex];
    if (!spec)
        return null;
    if (modifier === "'")
        return { ...spec, angle: -spec.angle };
    if (modifier === '2')
        return { ...spec, angle: spec.angle * 2 };
    return spec;
}
export class CubeRendererBridge {
    container;
    scene;
    camera;
    renderer;
    controls;
    cubies = new Map();
    stickers = new Map();
    pivot;
    animating = false;
    animationQueue = [];
    animationStartTime = 0;
    animationDuration = 400;
    currentMoveSpec = null;
    currentCubiesInLayer = [];
    animationFrameId = -1;
    nodes = new Map();
    nodeGroups = new Map();
    orbitPaths = new Map();
    connectionLines = new THREE.Group();
    orbitHighlight = null;
    showNodesFlag = true;
    showOrbitsFlag = false;
    showConnectionsFlag = false;
    constructor(containerId = 'app') {
        const el = document.getElementById(containerId);
        if (!el) {
            throw new Error(`Container element '#${containerId}' not found`);
        }
        this.container = el;
        this.pivot = new THREE.Group();
        this.init();
        this.animate();
    }
    handle(message) {
        switch (message.type) {
            case 'setState':
                this.setState(message.stickers ?? []);
                break;
            case 'animateMove':
                this.animateMove(message.move ?? '');
                break;
            case 'reset':
                this.reset();
                break;
            case 'toggleMathVis':
                this.toggleMathVis(message.showNodes, message.showOrbits, message.showConnections);
                break;
        }
    }
    init() {
        this.scene = new THREE.Scene();
        this.scene.background = new THREE.Color(0x102629);
        this.camera = new THREE.PerspectiveCamera(45, this.container.clientWidth / this.container.clientHeight, 0.1, 100);
        this.camera.position.set(6, 5, 8);
        this.camera.lookAt(0, 0, 0);
        this.renderer = new THREE.WebGLRenderer({ antialias: true, alpha: true });
        this.renderer.setSize(this.container.clientWidth, this.container.clientHeight);
        this.renderer.setPixelRatio(window.devicePixelRatio);
        this.container.appendChild(this.domElement);
        this.controls = new OrbitControls(this.camera, this.domElement);
        this.controls.enableDamping = true;
        this.controls.dampingFactor = 0.05;
        this.controls.rotateSpeed = 0.6;
        this.controls.minDistance = 4;
        this.controls.maxDistance = 15;
        const ambientLight = new THREE.AmbientLight(0xffffff, 0.6);
        this.scene.add(ambientLight);
        const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
        directionalLight.position.set(5, 10, 7);
        this.scene.add(directionalLight);
        this.buildCube();
        this.buildMathVis();
        window.addEventListener('resize', () => this.onResize());
    }
    get domElement() {
        return this.renderer.domElement;
    }
    buildCube() {
        this.cubies.forEach((cubie) => this.scene.remove(cubie));
        this.cubies.clear();
        this.stickers.clear();
        const cubieSize = 0.92;
        const stickerMargin = 0.04;
        const bodyGeo = new THREE.BoxGeometry(cubieSize - 0.08, cubieSize - 0.08, cubieSize - 0.08);
        const bodyMat = new THREE.MeshStandardMaterial({ color: 0x1a1a1a, roughness: 0.8 });
        const stickerGeo = new THREE.PlaneGeometry(cubieSize - stickerMargin, cubieSize - stickerMargin);
        for (let x = -1; x <= 1; x++) {
            for (let y = -1; y <= 1; y++) {
                for (let z = -1; z <= 1; z++) {
                    const cubie = new THREE.Group();
                    cubie.position.set(x, y, z);
                    const body = new THREE.Mesh(bodyGeo, bodyMat);
                    cubie.add(body);
                    const stickerDefs = [];
                    if (x === 1)
                        stickerDefs.push({ nx: 1, ny: 0, nz: 0, key: 'red' });
                    if (x === -1)
                        stickerDefs.push({ nx: -1, ny: 0, nz: 0, key: 'orange' });
                    if (y === 1)
                        stickerDefs.push({ nx: 0, ny: 1, nz: 0, key: 'white' });
                    if (y === -1)
                        stickerDefs.push({ nx: 0, ny: -1, nz: 0, key: 'yellow' });
                    if (z === 1)
                        stickerDefs.push({ nx: 0, ny: 0, nz: 1, key: 'green' });
                    if (z === -1)
                        stickerDefs.push({ nx: 0, ny: 0, nz: -1, key: 'blue' });
                    for (const def of stickerDefs) {
                        const sticker = new THREE.Mesh(stickerGeo, new THREE.MeshStandardMaterial({ color: COLOR_HEX[def.key] }));
                        sticker.position.set(def.nx * 0.02, def.ny * 0.02, def.nz * 0.02);
                        if (def.nx !== 0)
                            sticker.rotation.y = def.nx > 0 ? Math.PI / 2 : -Math.PI / 2;
                        if (def.ny !== 0)
                            sticker.rotation.x = def.ny > 0 ? -Math.PI / 2 : Math.PI / 2;
                        if (def.nz !== 0 && def.nz < 0)
                            sticker.rotation.y = Math.PI;
                        cubie.add(sticker);
                        this.stickers.set(`${x},${y},${z},${def.nx},${def.ny},${def.nz}`, sticker);
                    }
                    this.scene.add(cubie);
                    this.cubies.set(`${x},${y},${z}`, cubie);
                }
            }
        }
    }
    setState(stickers) {
        if (stickers.length !== 54) {
            console.warn(`Expected 54 stickers, got ${stickers.length}`);
            return;
        }
        for (let i = 0; i < 54; i++) {
            const faceIndex = Math.floor(i / 9);
            const cell = i % 9;
            const row = Math.floor(cell / 3);
            const col = cell % 3;
            const mapping = STICKER_MAP[`${faceIndex},${row},${col}`];
            if (!mapping)
                continue;
            const stickerKey = `${mapping.x},${mapping.y},${mapping.z},${mapping.nx},${mapping.ny},${mapping.nz}`;
            const sticker = this.stickers.get(stickerKey);
            if (sticker && sticker.material instanceof THREE.MeshStandardMaterial) {
                sticker.material.color.setHex(COLOR_HEX[stickers[i]]);
            }
        }
        this.updateMathVis(stickers);
    }
    animateMove(move) {
        const spec = parseMove(move);
        if (!spec) {
            console.warn(`Unknown move: ${move}`);
            return;
        }
        if (this.animating) {
            this.animationQueue.push(move);
            return;
        }
        this.startAnimation(spec, move);
    }
    reset() {
        if (this.animating) {
            this.animationQueue = [];
            this.finishAnimation();
        }
        const solved = [];
        for (let face = 0; face < 6; face++) {
            const color = ['white', 'yellow', 'green', 'blue', 'red', 'orange'][face];
            for (let i = 0; i < 9; i++)
                solved.push(color);
        }
        this.setState(solved);
    }
    startAnimation(spec, move) {
        this.animating = true;
        this.currentMoveSpec = spec;
        this.animationStartTime = performance.now();
        this.currentCubiesInLayer = [];
        const epsilon = 0.5;
        this.cubies.forEach((cubie, key) => {
            const [x, y, z] = key.split(',').map(Number);
            let inLayer = false;
            if (spec.axis === 'x' && Math.abs(x - spec.layer) < epsilon)
                inLayer = true;
            if (spec.axis === 'y' && Math.abs(y - spec.layer) < epsilon)
                inLayer = true;
            if (spec.axis === 'z' && Math.abs(z - spec.layer) < epsilon)
                inLayer = true;
            if (inLayer) {
                this.pivot.add(cubie);
                this.currentCubiesInLayer.push(cubie);
            }
        });
        this.pivot.position.set(0, 0, 0);
        this.pivot.rotation.set(0, 0, 0);
        this.scene.add(this.pivot);
        if (this.showOrbitsFlag)
            this.highlightOrbit(spec);
        this.animateFrame();
    }
    animateFrame() {
        const now = performance.now();
        const elapsed = now - this.animationStartTime;
        const t = Math.min(elapsed / this.animationDuration, 1);
        const easeT = 1 - (1 - t) * (1 - t);
        const angle = this.currentMoveSpec.angle * easeT;
        const axis = this.currentMoveSpec.axis;
        this.pivot.rotation.set(axis === 'x' ? angle : 0, axis === 'y' ? angle : 0, axis === 'z' ? angle : 0);
        if (t < 1) {
            this.animationFrameId = requestAnimationFrame(() => this.animateFrame());
        }
        else {
            this.finishAnimation();
        }
    }
    finishAnimation() {
        this.pivot.updateMatrixWorld();
        for (const cubie of this.currentCubiesInLayer) {
            cubie.applyMatrix4(this.pivot.matrixWorld);
        }
        this.pivot.rotation.set(0, 0, 0);
        this.pivot.position.set(0, 0, 0);
        this.scene.remove(this.pivot);
        this.animating = false;
        this.currentMoveSpec = null;
        this.currentCubiesInLayer = [];
        this.clearOrbitHighlight();
        if (this.animationQueue.length > 0) {
            const next = this.animationQueue.shift();
            const spec = parseMove(next);
            if (spec)
                this.startAnimation(spec, next);
        }
    }
    onResize() {
        this.camera.aspect = this.container.clientWidth / this.container.clientHeight;
        this.camera.updateProjectionMatrix();
        this.renderer.setSize(this.container.clientWidth, this.container.clientHeight);
    }
    animate = () => {
        this.animationFrameId = requestAnimationFrame(this.animate);
        this.controls.update();
        this.renderer.render(this.scene, this.camera);
    };
    buildMathVis() {
        const nodeGeo = new THREE.SphereGeometry(0.06, 8, 8);
        const orbitGeo = new THREE.RingGeometry(1.35, 1.45, 64);
        const faceColors = ['white', 'yellow', 'green', 'blue', 'red', 'orange'];
        for (const color of faceColors) {
            const group = new THREE.Group();
            this.nodeGroups.set(color, group);
            this.scene.add(group);
        }
        const orbitConfigs = [
            { position: [0, 1.3, 0], rotation: [Math.PI / 2, 0, 0] },
            { position: [1.3, 0, 0], rotation: [0, Math.PI / 2, 0] },
            { position: [0, 0, 1.3], rotation: [0, 0, 0] },
            { position: [0, -1.3, 0], rotation: [Math.PI / 2, 0, 0] },
            { position: [-1.3, 0, 0], rotation: [0, Math.PI / 2, 0] },
            { position: [0, 0, -1.3], rotation: [0, 0, 0] },
        ];
        for (let faceIndex = 0; faceIndex < 6; faceIndex++) {
            const faceColor = faceColors[faceIndex];
            const orbitMat = new THREE.MeshStandardMaterial({
                color: COLOR_HEX[faceColor],
                transparent: true,
                opacity: 0.12,
                side: THREE.DoubleSide,
            });
            const orbit = new THREE.Mesh(orbitGeo, orbitMat);
            const cfg = orbitConfigs[faceIndex];
            orbit.position.set(...cfg.position);
            orbit.rotation.set(...cfg.rotation);
            orbit.visible = this.showOrbitsFlag;
            this.orbitPaths.set(`${faceIndex}`, orbit);
            this.scene.add(orbit);
            for (let row = 0; row < 3; row++) {
                for (let col = 0; col < 3; col++) {
                    const mapping = STICKER_MAP[`${faceIndex},${row},${col}`];
                    if (!mapping)
                        continue;
                    const nodeKey = `${mapping.x},${mapping.y},${mapping.z},${mapping.nx},${mapping.ny},${mapping.nz}`;
                    const nodeMat = new THREE.MeshStandardMaterial({
                        color: COLOR_HEX[faceColor],
                        emissive: COLOR_HEX[faceColor],
                        emissiveIntensity: 0.25,
                    });
                    const node = new THREE.Mesh(nodeGeo, nodeMat);
                    node.position.set(mapping.x, mapping.y, mapping.z);
                    node.visible = this.showNodesFlag;
                    this.nodes.set(nodeKey, node);
                    this.nodeGroups.get(faceColor).add(node);
                }
            }
        }
        const lineMat = new THREE.LineBasicMaterial({ color: 0x555555, transparent: true, opacity: 0.35 });
        for (let faceIndex = 0; faceIndex < 6; faceIndex++) {
            for (let row = 0; row < 3; row++) {
                for (let col = 0; col < 3; col++) {
                    const m = STICKER_MAP[`${faceIndex},${row},${col}`];
                    if (!m)
                        continue;
                    if (col < 2) {
                        const right = STICKER_MAP[`${faceIndex},${row},${col + 1}`];
                        if (right)
                            this.addFaceConnectionLine(m, right, lineMat);
                    }
                    if (row < 2) {
                        const down = STICKER_MAP[`${faceIndex},${row + 1},${col}`];
                        if (down)
                            this.addFaceConnectionLine(m, down, lineMat);
                    }
                }
            }
        }
        this.connectionLines.visible = this.showConnectionsFlag;
        this.scene.add(this.connectionLines);
    }
    addFaceConnectionLine(a, b, material) {
        const geo = new THREE.BufferGeometry().setFromPoints([
            new THREE.Vector3(a.x, a.y, a.z),
            new THREE.Vector3(b.x, b.y, b.z),
        ]);
        this.connectionLines.add(new THREE.Line(geo, material));
    }
    updateMathVis(stickers) {
        for (let faceIndex = 0; faceIndex < 6; faceIndex++) {
            for (let row = 0; row < 3; row++) {
                for (let col = 0; col < 3; col++) {
                    const mapping = STICKER_MAP[`${faceIndex},${row},${col}`];
                    if (!mapping)
                        continue;
                    const nodeKey = `${mapping.x},${mapping.y},${mapping.z},${mapping.nx},${mapping.ny},${mapping.nz}`;
                    const node = this.nodes.get(nodeKey);
                    const color = stickers[faceIndex * 9 + row * 3 + col];
                    if (node && node.material instanceof THREE.MeshStandardMaterial) {
                        node.material.color.setHex(COLOR_HEX[color]);
                        node.material.emissive.setHex(COLOR_HEX[color]);
                    }
                }
            }
        }
    }
    toggleMathVis(showNodes, showOrbits, showConnections) {
        if (showNodes !== undefined) {
            this.showNodesFlag = showNodes;
            this.nodes.forEach((node) => { node.visible = showNodes; });
        }
        if (showOrbits !== undefined) {
            this.showOrbitsFlag = showOrbits;
            this.orbitPaths.forEach((orbit) => { orbit.visible = showOrbits; });
        }
        if (showConnections !== undefined) {
            this.showConnectionsFlag = showConnections;
            this.connectionLines.visible = showConnections;
        }
    }
    highlightOrbit(spec) {
        this.clearOrbitHighlight();
        const faceIndex = this.specToFaceIndex(spec);
        const orbit = this.orbitPaths.get(`${faceIndex}`);
        if (!orbit)
            return;
        const highlightGeo = new THREE.RingGeometry(1.40, 1.52, 64);
        const highlightMat = new THREE.MeshStandardMaterial({
            color: 0xffff00,
            transparent: true,
            opacity: 0.55,
            side: THREE.DoubleSide,
        });
        this.orbitHighlight = new THREE.Mesh(highlightGeo, highlightMat);
        this.orbitHighlight.position.copy(orbit.position);
        this.orbitHighlight.rotation.copy(orbit.rotation);
        this.scene.add(this.orbitHighlight);
    }
    clearOrbitHighlight() {
        if (this.orbitHighlight) {
            this.orbitHighlight.geometry.dispose();
            if (this.orbitHighlight.material instanceof THREE.MeshStandardMaterial) {
                this.orbitHighlight.material.dispose();
            }
            this.scene.remove(this.orbitHighlight);
            this.orbitHighlight = null;
        }
    }
    specToFaceIndex(spec) {
        if (spec.axis === 'x')
            return spec.layer === 1 ? 1 : 4;
        if (spec.axis === 'y')
            return spec.layer === 1 ? 0 : 3;
        return spec.layer === 1 ? 2 : 5;
    }
    dispose() {
        cancelAnimationFrame(this.animationFrameId);
        window.removeEventListener('resize', () => this.onResize());
        this.renderer.dispose();
        this.controls.dispose();
        if (this.orbitHighlight) {
            this.orbitHighlight.geometry.dispose();
            if (this.orbitHighlight.material instanceof THREE.MeshStandardMaterial) {
                this.orbitHighlight.material.dispose();
            }
        }
        this.nodes.forEach((node) => {
            node.geometry.dispose();
            if (node.material instanceof THREE.MeshStandardMaterial)
                node.material.dispose();
        });
        this.nodeGroups.forEach((group) => {
            group.traverse((obj) => {
                if (obj instanceof THREE.Mesh) {
                    obj.geometry.dispose();
                    if (obj.material instanceof THREE.MeshStandardMaterial)
                        obj.material.dispose();
                }
            });
        });
        this.orbitPaths.forEach((path) => {
            path.geometry.dispose();
            if (path.material instanceof THREE.MeshStandardMaterial)
                path.material.dispose();
        });
        this.connectionLines.traverse((obj) => {
            if (obj instanceof THREE.Line) {
                obj.geometry.dispose();
                if (obj.material instanceof THREE.LineBasicMaterial)
                    obj.material.dispose();
            }
        });
        this.scene.traverse((obj) => {
            if (obj instanceof THREE.Mesh) {
                obj.geometry.dispose();
                if (obj.material instanceof THREE.MeshStandardMaterial)
                    obj.material.dispose();
            }
        });
    }
}
function createBridge() {
    return new CubeRendererBridge('app');
}
const bridge = createBridge();
window.__cubeRendererBridge__ = bridge;
if (typeof window !== 'undefined' && window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', (event) => {
        try {
            const message = typeof event.data === 'string' ? JSON.parse(event.data) : event.data;
            bridge.handle(message);
        }
        catch (err) {
            console.error('Failed to handle renderer message:', err);
        }
    });
}
