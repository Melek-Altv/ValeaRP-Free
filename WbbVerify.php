<?php

require_once('global.php');
use wcf\data\user\User;
use wcf\data\user\group\UserGroup;

$code = 0;
$json = ["StatusCode" => $code, "UserData" => null];
checkPassword($_POST['Username'], $_POST['Password'], $_POST['Key']);

function checkPassword($username, $password, $key) {
	$secretKey = "DEINKEY"; //128-Character Key

	if (strcmp($key, $secretKey) != 0)
	{
		global $code;
		global $json;
		$code = 1;
		$json = ["statusCode" => $code, "userData" => null];
		return null;
	}
	if (empty($username) || empty($password)) {
		global $code;
		global $json;
		$code = 2;
		$json = ["statusCode" => $code, "userData" => null]; 
		return null;
	}
	$user = User::getUserByUsername($username);
	if(!$user->userID)
	{
		global $code;
		global $json;
		$code = 11;
		$json = ["statusCode" => $code, "userData" => null];
		return null;
	}
	else if (!$user->checkPassword($password)) {
		global $code;
		global $json;
		$code = 11;
		$json = ["statusCode" => $code, "userData" => null];
		return null;
	}
	else {
		global $code;
		global $json;
		$code = 10;
		$json = ["statusCode" => $code, "userData" =>  ["userId" => $user->userID, "username" => $user->username, "banned" => (bool)$user->banned, "banReason" => $user->banReason, "email" => $user->email]];
		return null;
	}
}

echo json_encode($json);
?>
