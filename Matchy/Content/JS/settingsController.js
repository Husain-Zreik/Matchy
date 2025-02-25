document.addEventListener("DOMContentLoaded", function () {
    const imageInput = document.getElementById("imageInput");
    const uploadButton = document.getElementById("uploadButton");
    const imagePreview = document.getElementById("imagePreview");
    const resetBtn = document.getElementById("resetImages");
    const dropZone = document.getElementById("dropZone");

    let filesSelected = false;

    dropZone.addEventListener("dragover", function (e) {
        e.preventDefault();
        dropZone.classList.add("dragover");
    });

    dropZone.addEventListener("dragleave", function () {
        dropZone.classList.remove("dragover");
    });

    dropZone.addEventListener("drop", function (e) {
        e.preventDefault();
        dropZone.classList.remove("dragover");
        const files = e.dataTransfer.files;
        if (files.length > 0) {
            const dataTransfer = new DataTransfer();
            for (let file of files) {
                dataTransfer.items.add(file);
            }
            imageInput.files = dataTransfer.files;
            filesSelected = true;
            displayUploadButton();
            previewImages();
        }
    });

    dropZone.addEventListener("click", function () {
        if (!filesSelected) imageInput.click();
    });

    imageInput.addEventListener("change", function () {
        if (imageInput.files.length > 0) {
            filesSelected = true;
            displayUploadButton();
            previewImages();
        }
    });

    function displayUploadButton() {
        uploadButton.style.display = "block";
    }

    function previewImages() {
        imagePreview.innerHTML = '';
        Array.from(imageInput.files).forEach((file, index) => {
            const reader = new FileReader();
            reader.onload = function (e) {
                const container = document.createElement("div");
                container.classList.add("image-container");

                const imgElement = document.createElement("img");
                imgElement.src = e.target.result;
                imgElement.classList.add("preview-img");

                const deleteButton = document.createElement("button");
                deleteButton.classList.add("btn-delete");
                deleteButton.textContent = "X";
                deleteButton.addEventListener("click", function () {
                    const newFileList = Array.from(imageInput.files);
                    newFileList.splice(index, 1);
                    const dataTransfer = new DataTransfer();
                    newFileList.forEach(file => dataTransfer.items.add(file));
                    imageInput.files = dataTransfer.files;
                    container.remove();
                    displayUploadButton();
                });

                container.appendChild(imgElement);
                container.appendChild(deleteButton);
                imagePreview.appendChild(container);
            };
            reader.readAsDataURL(file);
        });
    }

    uploadButton.addEventListener("click", async function (e) {
        e.preventDefault();
        if (!imageInput.files || imageInput.files.length === 0) {
            alert("Please select at least one image.");
            return;
        }

        try {
            let formData = new FormData();
            Array.from(imageInput.files).forEach(file => formData.append("files", file));

            const response = await fetch("/Settings/Upload", {
                method: "POST",
                body: formData
            });

            if (!response.ok) throw new Error("Server error occurred during upload.");

            const data = await response.json();
            if (data.success) {
                uploadButton.style.display = "none";
                alert("Images uploaded successfully!");
                imagePreview.innerHTML = '';
                imageInput.value = '';
            } else {
                alert("Upload failed: " + data.message);
            }
        } catch (error) {
            alert("Upload failed due to an error.");
        }
    });

    resetBtn.addEventListener("click", async function () {
        try {
            const response = await fetch("/Settings/Reset", { method: "POST" });
            const data = await response.json();
            if (data.success) {
                alert("All images have been reset.");
                imagePreview.innerHTML = '';
                uploadButton.style.display = "none";
            } else {
                alert("Reset failed.");
            }
        } catch (error) {
            console.error("Error:", error);
        }
    });
});