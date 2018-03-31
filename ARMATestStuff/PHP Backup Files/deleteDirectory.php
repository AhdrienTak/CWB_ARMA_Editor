<?php

function removeDirectory($dir) { 
	if (is_dir($dir)) { 
		$objects = scandir($dir); 
		foreach ($objects as $object) { 
			if ($object != '.' && $object != '..') { 
				if (is_dir($dir . '/' . $object))
					removeDirectory($dir . '/' . $object);
				else
					unlink($dir . '/' . $object); 
			} 
		}
		reset($objects);
		rmdir($dir); 
	} 
}

if (isset($_POST["author"]) && isset($_POST["type"]) && isset($_POST["gallery"]) && isset($_POST['name'])) {

	if (strcmp($_POST['type'], 'Gallery') == 0) {
		$filePath = '_' . $_POST['author'] . '_' . $_POST['name'];
		if (is_dir($filePath)) {
			removeDirectory($filePath);
		}
		else {
			die("Error: Directory does not exist.");
		}
	}
	
	else if (strcmp($_POST['type'], 'Piece') == 0) {
		$filePath = '_' . $_POST['author'] . '_' . $_POST['gallery'] . '/' . '_' . $_POST['name'];
		if (is_dir($filePath)) {
			removeDirectory($filePath);
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