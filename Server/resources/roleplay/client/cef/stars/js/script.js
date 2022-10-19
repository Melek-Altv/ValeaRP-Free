function UpdateStarsCef(currPoliceMembers) {
    if (currPoliceMembers <= 0) {
        $("#star1").attr("src", "./img/blank.png");
        $("#star2").attr("src", "./img/blank.png");
        $("#star3").attr("src", "./img/blank.png");
        $("#star4").attr("src", "./img/blank.png");
        $("#star5").attr("src", "./img/blank.png");
    } else if (currPoliceMembers == 1) {
        $("#star1").attr("src", "./img/full.png");
        $("#star2").attr("src", "./img/blank.png");
        $("#star3").attr("src", "./img/blank.png");
        $("#star4").attr("src", "./img/blank.png");
        $("#star5").attr("src", "./img/blank.png");
    } else if (currPoliceMembers == 2 ) {
        $("#star1").attr("src", "./img/full.png");
        $("#star2").attr("src", "./img/full.png");
        $("#star3").attr("src", "./img/blank.png");
        $("#star4").attr("src", "./img/blank.png");
        $("#star5").attr("src", "./img/blank.png");
    } else if (currPoliceMembers == 3) {
        $("#star1").attr("src", "./img/full.png");
        $("#star2").attr("src", "./img/full.png");
        $("#star3").attr("src", "./img/full.png");
        $("#star4").attr("src", "./img/blank.png");
        $("#star5").attr("src", "./img/blank.png");
    } else if (currPoliceMembers == 4) {
        $("#star1").attr("src", "./img/full.png");
        $("#star2").attr("src", "./img/full.png");
        $("#star3").attr("src", "./img/full.png");
        $("#star4").attr("src", "./img/full.png");
        $("#star5").attr("src", "./img/blank.png");
    } else if (currPoliceMembers == 5) {
        $("#star1").attr("src", "./img/full.png");
        $("#star2").attr("src", "./img/full.png");
        $("#star3").attr("src", "./img/full.png");
        $("#star4").attr("src", "./img/full.png");
        $("#star5").attr("src", "./img/full.png");
    }
}

function ShowStarsCef (currPoliceMembers) {
    $('body').css("display", "block");
    if (currPoliceMembers <= 0) {
        $("#star1").attr("src", "./img/blank.png");
        $("#star2").attr("src", "./img/blank.png");
        $("#star3").attr("src", "./img/blank.png");
        $("#star4").attr("src", "./img/blank.png");
        $("#star5").attr("src", "./img/blank.png");
    }
    else if (currPoliceMembers == 1) {
        $("#star1").attr("src", "./img/full.png");
        $("#star2").attr("src", "./img/blank.png");
        $("#star3").attr("src", "./img/blank.png");
        $("#star4").attr("src", "./img/blank.png");
        $("#star5").attr("src", "./img/blank.png");
    } else if (currPoliceMembers == 2 ) {
        $("#star1").attr("src", "./img/full.png");
        $("#star2").attr("src", "./img/full.png");
        $("#star3").attr("src", "./img/blank.png");
        $("#star4").attr("src", "./img/blank.png");
        $("#star5").attr("src", "./img/blank.png");
    } else if (currPoliceMembers == 3) {
        $("#star1").attr("src", "./img/full.png");
        $("#star2").attr("src", "./img/full.png");
        $("#star3").attr("src", "./img/full.png");
        $("#star4").attr("src", "./img/blank.png");
        $("#star5").attr("src", "./img/blank.png");
    } else if (currPoliceMembers == 4) {
        $("#star1").attr("src", "./img/full.png");
        $("#star2").attr("src", "./img/full.png");
        $("#star3").attr("src", "./img/full.png");
        $("#star4").attr("src", "./img/full.png");
        $("#star5").attr("src", "./img/blank.png");
    } else if (currPoliceMembers == 5) {
        $("#star1").attr("src", "./img/full.png");
        $("#star2").attr("src", "./img/full.png");
        $("#star3").attr("src", "./img/full.png");
        $("#star4").attr("src", "./img/full.png");
        $("#star5").attr("src", "./img/full.png");
    }
}

alt.on("CEF:Stars:UpdateStarsCef", (currPoliceMembers) => {
    UpdateStarsCef(currPoliceMembers);
});

alt.on("CEF:Stars:CreateStarsCEF", (currPoliceMembers) => {
    ShowStarsCef(currPoliceMembers);
});