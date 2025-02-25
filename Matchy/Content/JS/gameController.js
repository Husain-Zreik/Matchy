let currentLevel = 1, mistakes = 0, score = 0, correctMatches = 0, totalCardPairs = 3, elapsedTime = 0;
const playerName = 'player'; // Player name
let timer, board, startBtn, resetBtn, mistakesLabel, scoreLabel, timeLabel, levelLabel;
let memorizationTime = 3, cardValues = [], customImages = [], shuffledCardValues = [];
let firstClicked = null, secondClicked = null, isChecking = false;

window.onload = function () {
    // DOM element references
    board = document.getElementById('board');
    startBtn = document.getElementById('startBtn');
    resetBtn = document.getElementById('resetBtn');
    mistakesLabel = document.getElementById('lblScore');
    scoreLabel = document.getElementById('lblLiveScore');
    timeLabel = document.getElementById('lblTimer');
    levelLabel = document.getElementById('lblLevel');
};

function startGame() {
    fetchCustomImages().then(fetchedImages => {
        customImages = fetchedImages;
        startBtn.disabled = true;
        resetBtn.disabled = false;
        resetGameStats();
        createBoard();

        displayCards();
        setTimeout(() => {
            hideCards();
            startTimer();
        }, memorizationTime * 1000);
    });
}

function resetGame() {
    clearInterval(timer);
    startBtn.disabled = false;
    resetBtn.disabled = true;
    currentLevel = 1;
    totalCardPairs = 3;
    correctMatches = 0;
    elapsedTime = 0;
    updateStats();
    board.innerHTML = '';
    showRatingModal();
}

function updateStats() {
    mistakesLabel.textContent = mistakes;
    scoreLabel.textContent = score;
    levelLabel.textContent = currentLevel;
    timeLabel.textContent = new Date(elapsedTime * 1000).toISOString().substr(14, 5);
}

function resetGameStats() {
    correctMatches = 0;
    mistakes = 0;
    score = 0;
    elapsedTime = 0;
    updateStats();
}

function createBoard() {
    cardValues = [];
    shuffledCardValues = [];
    fetchCustomImages().then(() => {
        let customImageCount = customImages.length;

        for (let i = 1; i <= totalCardPairs; i++) {
            if (i <= customImageCount) {
                cardValues.push(customImages[i - 1], customImages[i - 1]);
            } else {
                cardValues.push(`image_${i}.jpg`, `image_${i}.jpg`);
            }
        }

        shuffledCardValues = shuffleArray(cardValues);
        displayCards();
    });
}

function fetchCustomImages() {
    return fetch('/Settings/GetCustomImages')
        .then(response => response.json())
        .then(data => data.success ? data.images : [])
        .catch(error => {
            console.error("Error fetching custom images:", error);
            return [];
        });
}

function shuffleArray(arr) {
    return arr.sort(() => Math.random() - 0.5);
}

function displayCards() {
    board.innerHTML = '';
    shuffledCardValues.forEach((image, index) => {
        let imageUrl = customImages.length > 0 && customImages.includes(image)
            ? `/Content/CustomImages/${image}`
            : `/Content/Images/${image}`;

        let card = document.createElement('div');
        card.classList.add('game-card');
        card.setAttribute('data-image', image);
        card.style.backgroundImage = `url('${imageUrl}')`;
        card.classList.add('flipped');
        card.addEventListener('click', cardClick);
        board.appendChild(card);
    });
}

function hideCards() {
    document.querySelectorAll('.game-card').forEach(card => {
        card.style.backgroundImage = '';
        card.classList.remove('flipped');
        card.addEventListener('click', cardClick);
    });
}

function cardClick(event) {
    const clickedCard = event.target;
    if (isChecking || clickedCard === firstClicked || clickedCard.classList.contains('flipped')) return;

    const image = clickedCard.getAttribute('data-image');
    const imageUrl = customImages.length > 0 && customImages.includes(image)
        ? `/Content/CustomImages/${image}`
        : `/Content/Images/${image}`;
    clickedCard.style.backgroundImage = `url('${imageUrl}')`;
    clickedCard.classList.add('flipped');

    if (!firstClicked) {
        firstClicked = clickedCard;
        return;
    }

    secondClicked = clickedCard;
    isChecking = true;
    checkMatch();
}

function checkMatch() {
    if (firstClicked.getAttribute('data-image') === secondClicked.getAttribute('data-image')) {
        correctMatches++;
        score += 100;
        firstClicked = null;
        secondClicked = null;
        isChecking = false;

        if (correctMatches === totalCardPairs) {
            clearInterval(timer);
            if (playerName) saveScoreToServer(score, currentLevel);

            setTimeout(() => {
                if (currentLevel === 4) {
                    alert(`🎉 Congratulations! You've completed all levels! 🎉`);
                    resetGame();
                } else {
                    alert(`Level ${currentLevel} Complete! Moving to Level ${currentLevel + 1}.`);
                    currentLevel++;
                    totalCardPairs = currentLevel * 3;
                    resetBoardForNextLevel();
                }
            }, 500);
        }
    } else {
        mistakes++;
        score -= 10;
        setTimeout(() => {
            firstClicked.style.backgroundImage = '';
            secondClicked.style.backgroundImage = '';
            firstClicked.classList.remove('flipped');
            secondClicked.classList.remove('flipped');
            firstClicked = null;
            secondClicked = null;
            isChecking = false;
            updateStats();
        }, 1000);
    }

    updateStats();
}

function resetBoardForNextLevel() {
    correctMatches = 0;
    elapsedTime = 0;
    updateStats();
    createBoard();

    displayCards();
    setTimeout(() => {
        hideCards();
        startTimer();
    }, memorizationTime * 1000);
}

function startTimer() {
    timer = setInterval(() => {
        elapsedTime++;
        updateStats();
    }, 1000);
}

function saveScoreToServer(score, currentLevel) {
    fetch('/Game/SaveScore', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ playerName, score, level: currentLevel })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log("Score saved successfully.");
            } else {
                console.error('Error saving score:', data.message);
            }
        })
        .catch(error => console.error('Error:', error));
}

function showRatingModal() {
    Swal.fire({
        title: 'Rate the Game',
        html: `
            <div class="stars">
                <span class="star" data-value="1">&#9733;</span>
                <span class="star" data-value="2">&#9733;</span>
                <span class="star" data-value="3">&#9733;</span>
                <span class="star" data-value="4">&#9733;</span>
                <span class="star" data-value="5">&#9733;</span>
            </div>
        `,
        showCancelButton: true,
        confirmButtonText: 'Submit Rating',
        preConfirm: () => {
            const rating = document.querySelectorAll('.star.checked').length;
            return rating;
        }
    }).then((result) => {
        if (result.isConfirmed) {
            const rating = document.querySelectorAll('.star.checked').length;
            Swal.fire('Thanks for your rating!', '', 'success');
            saveRatingToServer(rating);
        }
    });

    document.querySelectorAll('.star').forEach(star => {
        star.addEventListener('click', (e) => {
            const value = e.target.getAttribute('data-value');
            document.querySelectorAll('.star').forEach(s => {
                s.classList.toggle('checked', parseInt(s.getAttribute('data-value')) <= value);
            });
        });
    });
}

function saveRatingToServer(rating) {
    if (playerName && rating >= 1 && rating <= 5) {
        fetch('/Game/SaveRating', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ playerName, rating })
        })
            .then(response => response.json())
            .then(data => {
                console.log(data.success ? "Rating saved" : "Error saving rating");
            })
            .catch(error => console.error('Error:', error));
    } else {
        console.error("Invalid rating or player name.");
    }
}
