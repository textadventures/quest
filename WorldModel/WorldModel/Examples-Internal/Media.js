function AddYouTube(id) {
    var embedHTML = "<object width=\"425\" height=\"344\"><param name=\"movie\" value=\"http://www.youtube.com/v/" + id + "\"></param><param name=\"allowFullScreen\" value=\"true\"></param><param name=\"allowscriptaccess\" value=\"always\"></param><embed src=\"http://www.youtube.com/v/" + id + "\" type=\"application/x-shockwave-flash\" allowscriptaccess=\"always\" allowfullscreen=\"true\" width=\"425\" height=\"344\"></embed></object>"
    document.body.innerHTML += embedHTML
}