AC_DEFUN([FSPOT_CHECK_FLICKRNET],
[
	FLICKRNET_REQUIRED=2.0

	PKG_CHECK_MODULES(FLICKRNET,
		flickrnet >= $FLICKRNET_REQUIRED)
	AC_SUBST(FLICKRNET_LIBS)
])
