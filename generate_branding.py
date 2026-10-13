
from pathlib import Path
from PIL import Image, ImageOps

ROOT = Path(__file__).resolve().parent
ASSETS = ROOT / "desktop_template" / "WebsiteDesktop" / "Assets"

SOURCE = ASSETS / "logo.png"
ICON = ASSETS / "app.ico"
SPLASH = ASSETS / "splash-logo.png"

def generate():
    if not SOURCE.exists():
        raise FileNotFoundError(
            f"Put your source logo here: {SOURCE}"
        )

    ASSETS.mkdir(parents=True, exist_ok=True)

    with Image.open(SOURCE) as original:
        logo = ImageOps.exif_transpose(original).convert("RGBA")

        # Create a square Windows icon without stretching the logo.
        fitted = ImageOps.contain(
            logo, (256, 256), Image.Resampling.LANCZOS
        )
        canvas = Image.new("RGBA", (256, 256), (0, 0, 0, 0))
        canvas.alpha_composite(
            fitted,
            ((256 - fitted.width) // 2, (256 - fitted.height) // 2),
        )

        canvas.save(
            ICON,
            format="ICO",
            sizes=[(16, 16), (24, 24), (32, 32),
                   (48, 48), (64, 64), (128, 128), (256, 256)],
        )

        # Make a larger transparent logo for the loading screen.
        splash = ImageOps.contain(
            logo, (600, 240), Image.Resampling.LANCZOS
        )
        splash.save(SPLASH, format="PNG")

    print(f"Generated: {ICON}")
    print(f"Generated: {SPLASH}")

if __name__ == "__main__":
    generate()
