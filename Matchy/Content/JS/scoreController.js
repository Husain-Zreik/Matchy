document.addEventListener("DOMContentLoaded", function () {
    let showingHighScores = false;

    loadPersonalScores();

    // Fetch personal scores
    async function loadPersonalScores() {
        try {
            const response = await fetch('/Score/AllScores', {
                method: 'GET',
                headers: { 'Accept': 'application/json' }
            });

            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

            const scores = await response.json();
            updateScoreTable(scores);

            const currentPlayerNameElement = document.getElementById("currentPlayerName");
            if (currentPlayerNameElement && scores.length > 0) {
                currentPlayerNameElement.textContent = scores[0].username;
            }
        } catch (error) {
            console.error('Error fetching personal scores:', error);
            document.querySelector("#scoreTable tbody").innerHTML =
                '<tr><td colspan="4">Error loading scores. Please try again later.</td></tr>';
        }
    }

    document.getElementById("toggleScores").addEventListener("click", function () {
        if (showingHighScores) {
            this.textContent = "Show High Scores";
            document.getElementById("title").textContent = "Score Board: My Scores";
            loadPersonalScores();
        } else {
            this.textContent = "Show My Scores";
            document.getElementById("title").textContent = "Score Board: High Scores";
            loadHighScores();
        }
        showingHighScores = !showingHighScores;
    });

    // Fetch high scores
    async function loadHighScores() {
        try {
            const response = await fetch('/Score/HighScores', {
                method: 'GET',
                headers: { 'Accept': 'application/json' }
            });

            if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

            const scores = await response.json();
            updateScoreTable(scores);
        } catch (error) {
            console.error('Error fetching high scores:', error);
            document.querySelector("#scoreTable tbody").innerHTML =
                '<tr><td colspan="4">Error loading high scores. Please try again later.</td></tr>';
        }
    }

    // Format date safely
    function formatDate(dateString) {
        try {
            if (!dateString) return "N/A";
            const msDateMatch = dateString.match(/\/Date\((\d+)\)\//);
            if (msDateMatch) {
                const timestamp = parseInt(msDateMatch[1], 10);
                return new Date(timestamp).toLocaleString();
            }
            const date = new Date(dateString);
            return isNaN(date.getTime()) ? "Invalid Date" : date.toLocaleString();
        } catch (error) {
            console.error("Error formatting date:", error);
            return "Date Error";
        }
    }

    // Update score table
    function updateScoreTable(scores) {
        const tbody = document.querySelector("#scoreTable tbody");
        tbody.innerHTML = "";

        if (!scores || scores.length === 0) {
            tbody.innerHTML = '<tr><td colspan="4">No scores available</td></tr>';
        } else {
            scores.forEach(score => {
                const row = document.createElement("tr");
                const username = score.username || "Unknown";
                const level = score.level || 0;
                const scoreValue = score.scoreValue || 0;
                const achievedAt = formatDate(score.achievedAt);

                row.innerHTML = `
                    <td>${username}</td>
                    <td>${level}</td>
                    <td>${scoreValue} points</td>
                    <td>${achievedAt}</td>
                `;
                tbody.appendChild(row);
            });
        }
    }

    // Reset all scores
    document.getElementById("resetScores").addEventListener("click", async function () {
        if (confirm("Are you sure you want to reset all scores? This action cannot be undone.")) {
            try {
                const response = await fetch('/Score/ResetScores', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'Accept': 'application/json'
                    }
                });

                if (!response.ok) throw new Error(`HTTP error! Status: ${response.status}`);

                const result = await response.json();
                if (result.success) {
                    alert("Scores have been reset successfully.");
                    loadPersonalScores();
                } else {
                    alert(result.message || "Error while resetting scores.");
                }
            } catch (error) {
                console.error('Error while resetting scores:', error);
                alert("Error while resetting scores.");
            }
        }
    });
});
