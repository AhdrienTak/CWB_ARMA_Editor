<?php

if (isset($_POST["author"]) && isset($_POST["type"]) && isset($_POST["gallery"]) && isset($_POST['name'])) {

	if (strcmp($_POST['type'], 'Gallery') == 0) {
		$filePath = '_' . $_POST['author'] . '_' . $_POST['name'];
		if (!is_dir($filePath)) {
			mkdir($filePath, 0755, true);
		}
		else {
			die("Error: Directory already exists.");
		}
	}
	
	else if (strcmp($_POST['type'], 'Piece') == 0) {
		$filePath = '_' . $_POST['author'] . '_' . $_POST['gallery'] . '/' . '_' . $_POST['name'];
		if (!is_dir($filePath)) {
			mkdir($filePath, 0755, true);
		}
		else {
			die("Error: Directory already exists.");
		}
	}

	else {
		die("Error: Incorrect directory type.");
	}

}

else {
	die("Error: Bad form for directory creation.");
}

?>