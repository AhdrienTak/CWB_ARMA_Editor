<?php

function removeDirectory($dir) { 
	if (is_dir($dir)) { 
		$objects = scandir($dir); 
		foreach ($objects as $object) { 
			if ($object != '.' && $object != '..') {
				if (strcmp($object, 'dimension.txt') == 0) {
					$txt_file = file_get_contents($dir . '/' . $object);
					$tokens   = explode(",", $txt_file);
					echo($tokens[8] . ',');
				}
				if (is_dir($dir . '/' . $object)) {
					removeDirectory($dir . '/' . $object);
				}
				else {
					unlink($dir . '/' . $object);
				}					
			} 
		}
		reset($objects);
		rmdir($dir); 
	} 
}

if (isset($_POST["author"]) && isset($_POST["type"]) && isset($_POST["gallery"]) && isset($_POST['name'])) {

	if (strcmp($_POST['type'], 'Gallery') == 0) {
		$filePath = '_' . $_POST['author'] . '_' . $_POST['name'];
		if (is_dir($filePath . 'x')) {
			removeDirectory($filePath . 'x');
		}
		else if (is_dir($filePath . 'o')) {
			removeDirectory($filePath . 'o');
		}
		else {
			die("Error: Directory does not exist.");
		}
	}
	
	else if (strcmp($_POST['type'], 'Piece') == 0) {
		$galleryPath = '_' . $_POST['author'] . '_' . $_POST['gallery'];
		if (is_dir($galleryPath . 'x/_' . $_POST['name'] . 'x')) {
			removeDirectory($galleryPath . 'x/_' . $_POST['name'] . 'x');
		}
		else if (is_dir($galleryPath . 'o/_' . $_POST['name'] . 'x')) {
			removeDirectory($galleryPath . 'o/_' . $_POST['name'] . 'x');
		}
		else if (is_dir($galleryPath . 'x/_' . $_POST['name'] . 'o')) {
			removeDirectory($galleryPath . 'x/_' . $_POST['name'] . 'o');
		}
		else if (is_dir($galleryPath . 'o/_' . $_POST['name'] . 'o')) {
			removeDirectory($galleryPath . 'o/_' . $_POST['name'] . 'o');
		}
		else {
			die("Error: Directory does not exist.");
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