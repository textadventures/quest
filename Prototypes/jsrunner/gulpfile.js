var gulp = require('gulp');  
var sourcemaps = require('gulp-sourcemaps');  
var ts = require('gulp-typescript');  
var babel = require('gulp-babel');
var uglify = require('gulp-uglify');

// when gulp-typescript works with TS1.6, this can be replaced with:
//    var tsProject = ts.createProject('./tsconfig.json');
// and the dev dependency on Microsoft/TypeScript can be removed from package.json

var tsProject = ts.createProject('./tsconfig.json', {
    typescript: require('typescript')
});

gulp.task('default', function() {
    gulp.src('./node_modules/gulp-babel/node_modules/babel-core/browser-polyfill.min.js')
        .pipe(gulp.dest('.'));
      
    return gulp.src('asl4.ts')
        .pipe(sourcemaps.init())
        .pipe(ts(tsProject))
        .pipe(babel())
        //.pipe(uglify())
        .pipe(sourcemaps.write('.'))
        .pipe(gulp.dest('.'));
});