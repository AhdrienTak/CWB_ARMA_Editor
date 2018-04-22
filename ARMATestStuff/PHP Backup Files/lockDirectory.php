<?php

if (isset($_POST["type"]) && isset($_POST["author"]) && isset($_POST["gallery"]) && isset($_POST['lock'])) {

	// Determine new Lock state
	$lockState;
	if (strcmp($_POST['lock'], 'x') == 0) {
		$lockState = 'o';
	}
	else {
		$lockState = 'x';
	}

	// If locking a gallery
	if (strcmp($_POST['type'], 'Gallery') == 0) {
		if      (is_dir('_' . $_POST['author'] . '_' . $_POST['gallery'] . $_POST['lock'])) {
			rename(
				'_' . $_POST['author'] . '_' . $_POST['gallery'] . $_POST['lock'],
				'_' . $_POST['author'] . '_' . $_POST['gallery'] . $lockState
			);
		}
		else {
			die("Error: Bad gallery locking POST.");
		}
	}

	// If locking a piece
	else if (isset($_POST["piece"]) && strcmp($_POST['type'], 'Piece') == 0) {
		if      (is_dir('_' . $_POST['author'] . '_' . $_POST['gallery'] . 'o/_' . $_POST['piece'] . $_POST['lock'])) {
			rename(
				'_' . $_POST['author'] . '_' . $_POST['gallery'] . 'o/_' . $_POST['piece'] . $_POST['lock'],
				'_' . $_POST['author'] . '_' . $_POST['gallery'] . 'o/_' . $_POST['piece'] . $lockState
			);
		}
		else if (is_dir('_' . $_POST['author'] . '_' . $_POST['gallery'] . 'x/_' . $_POST['piece'] . $_POST['lock'])) {
			rename(
				'_' . $_POST['author'] . '_' . $_POST['gallery'] . 'x/_' . $_POST['piece'] . $_POST['lock'],
				'_' . $_POST['author'] . '_' . $_POST['gallery'] . 'x/_' . $_POST['piece'] . $lockState
			);
		}
		else {
			die("Error: Bad piece locking POST.");
		}
	}
	
	// Else, failure
	else {
		die("Error: Bad form POST.");
	}
}

else {
	die("Error: Bad form for directory creation.");
}

// report success
if (strcmp($lockState, 'x') == 0) {
	die("Directory is no longer visible to the public.");
}
else {
	die("Directory is live for public viewing.");
}

?>