<?php

if (isset($_POST['author']) && isset($_POST['type']) && isset($_POST['gallery'])) {

	$outputList = "";
	$directories;
	
	if (strcmp($_POST['type'], 'Gallery') == 0) {
		$directories = glob('_' . $_POST['author'] . '*', GLOB_ONLYDIR);
	}
	
	else if (strcmp($_POST['type'], 'Piece') == 0) {
		if (is_dir('_' . $_POST['author'] . '_' . $_POST['gallery'] . 'x')) {
			$directories = glob('_' . $_POST['author'] . '_' . $_POST['gallery'] . 'x/' . '*', GLOB_ONLYDIR);
		}
		else {
			$directories = glob('_' . $_POST['author'] . '_' . $_POST['gallery'] . 'o/' . '*', GLOB_ONLYDIR);
		}
	}

	else {
		die("Error: Incorrect directory type.");
	}
	
	foreach ($directories as $directory) {
		
		if (strcmp($_POST['type'], 'Gallery') == 0) {
			$outputList .= substr($directory, (2 + strlen($_POST['author'])));
		}
	
		else if (strcmp($_POST['type'], 'Piece') == 0) {
			$outputList .= substr($directory, (5 + strlen($_POST['author']) + strlen($_POST['gallery'])));
		}
		
		$outputList .= ",";
	}
	
	die($outputList);
}

else {
	die("Error: Bad form for refresh listings.");
}

?>