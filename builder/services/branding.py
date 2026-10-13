
from io import BytesIO
from pathlib import Path

from PIL import Image, ImageOps


ICON_SIZES = [16, 32, 48, 64, 128, 256]


def make_square_icon(uploaded_file, output_path):
    """Create a Windows .ico file from an uploaded logo."""
    uploaded_file.seek(0)

    with Image.open(uploaded_file) as source:
        image = ImageOps.exif_transpose(source).convert("RGBA")

        # Fit the logo into a square canvas without stretching it.
        fitted = ImageOps.contain(image, (256, 256))

        canvas = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
        x = (256 - fitted.width) // 2
        y = (256 - fitted.height) // 2
        canvas.alpha_composite(fitted, (x, y))

        canvas.save(
            output_path,
            format="ICO",
            sizes=[(size, size) for size in ICON_SIZES],
        )


def make_splash_logo(uploaded_file, output_path):
    """Create a clean transparent logo for a loading screen."""
    uploaded_file.seek(0)

    with Image.open(uploaded_file) as source:
        image = ImageOps.exif_transpose(source).convert("RGBA")
        image.thumbnail((600, 240), Image.Resampling.LANCZOS)
        image.save(output_path, format="PNG")
