import * as THREE from 'https://cdn.jsdelivr.net/npm/three@0.180.0/build/three.module.js';
import { GLTFLoader } from 'https://cdn.jsdelivr.net/npm/three@0.180.0/examples/jsm/loaders/GLTFLoader.js';

console.log("Vision scene loaded");

let model = null;
let originalScale = null;

// Elements
const canvas = document.getElementById('vision-canvas');
const container = canvas.parentElement;

// Scene
const scene = new THREE.Scene();
scene.background = new THREE.Color(0x020212);

// Camera
const camera = new THREE.PerspectiveCamera(
    35,
    container.clientWidth / container.clientHeight,
    0.1,
    100
);
camera.position.set(0, 0, 6);

// Renderer
const renderer = new THREE.WebGLRenderer({
    canvas,
    antialias: true
});

renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
renderer.setSize(container.clientWidth, container.clientHeight);

// Lighting
scene.add(new THREE.AmbientLight(0xffffff, 10));

const directionalLight = new THREE.DirectionalLight(0xffffff, 7);
directionalLight.position.set(3, 5, 5);
scene.add(directionalLight);

// Load Vision model
const loader = new GLTFLoader();

loader.load(
    '/models/vision.glb',
    (gltf) => {
        model = gltf.scene;

        scene.add(model);

        // Centre the model
        const box = new THREE.Box3().setFromObject(model);
        const center = box.getCenter(new THREE.Vector3());

        model.position.sub(center);
        model.rotation.x = THREE.MathUtils.degToRad(100);
        model.rotation.z = THREE.MathUtils.degToRad(-10);
        originalScale = model.scale


        // Fit model into camera view
        const size = box.getSize(new THREE.Vector3());
        const maxDimension = Math.max(size.x, size.y, size.z);

        camera.position.z = maxDimension * 2.2;
        camera.lookAt(0, 0, 0);

        console.log("Vision model loaded", model);
    },
    undefined,
    (error) => {
        console.error("Failed to load Vision model:", error);
    }
);

// Resize
const resizeObserver = new ResizeObserver(() => {
    const width = container.clientWidth;
    const height = container.clientHeight;

    if (model && originalScale) {
        const scaleFactor = THREE.MathUtils.clamp(width / 500, 0.15, 1);
        model.scale = originalScale.multiplyScalar(scaleFactor);

        console.debug(model.scale);
    }
    camera.aspect = width / height;
    camera.updateProjectionMatrix();

    renderer.setSize(width, height);
});

resizeObserver.observe(container);

// Animation
function animate() {
    requestAnimationFrame(animate);
    renderer.render(scene, camera);
}

animate();