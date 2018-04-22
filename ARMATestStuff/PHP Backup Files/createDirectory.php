<?php

if (isset($_POST["author"]) && isset($_POST["type"]) && isset($_POST["gallery"]) && isset($_POST['name'])) {

	if (strcmp($_POST['type'], 'Gallery') == 0) {
		$filePath = '_' . $_POST['author'] . '_' . $_POST['name'];
		if (is_dir($filePath . 'x')) {
			die("Error: Directory already exists.");
		}
		else if (is_dir($filePath . 'o')) {
			die("Error: Directory already exists.");
		}
		else {
			mkdir($filePath . 'x', 0755, true);
			die("Created new gallery.");
		}
	}
	
	else if (strcmp($_POST['type'], 'Piece') == 0) {
		$filePath = '_' . $_POST['author'] . '_' . $_POST['gallery'];
		if (file_exists($filePath . 'o') && is_dir($filePath . 'o')) {
			if (is_dir($filePath . 'o/_' . $_POST['name'] . 'x') || is_dir($filePath . 'o/_' . $_POST['name'] . 'o')) {
				die("Error: Directory already exists.");
			}
			else {
				mkdir($filePath . 'o/_' . $_POST['name'] . 'x', 0755, true);
				die("Created new piece.");
			}
		}
		else if (file_exists($filePath . 'x') && is_dir($filePath . 'x')) {
			if (is_dir($filePath . 'x/_' . $_POST['name'] . 'x') || is_dir($filePath . 'x/_' . $_POST['name'] . 'o')) {
				die("Error: Directory already exists.");
			}
			else {
				mkdir($filePath . 'x/_' . $_POST['name'] . 'x', 0755, true);
				die("Created new piece.");
			}
		}
		else {
			die("Error: Selected gallery does not exist!");
		}
	}

	else {
		die("Error: Incorrect directory type.");
	}

}

else {
	die("Error: Bad form for directory creation.");
}

// Report success
die("Directory created!");

?>