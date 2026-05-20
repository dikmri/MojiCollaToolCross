
/// 同名ファイルが存在する場合、"name(1).ext" "name(2).ext" と連番を付けて
/// 使用可能なパスを返す。
fn find_available_path(path: &std::path::Path) -> std::path::PathBuf {
    if !path.exists() {
        return path.to_path_buf();
    }
    let parent = path.parent().unwrap_or(std::path::Path::new("."));
    let stem = path.file_stem().and_then(|s| s.to_str()).unwrap_or("file");
    let ext = path
        .extension()
        .and_then(|e| e.to_str())
        .map(|e| format!(".{}", e))
        .unwrap_or_default();

    for i in 1..=9999 {
        let candidate = parent.join(format!("{}({}){}", stem, i, ext));
        if !candidate.exists() {
            return candidate;
        }
    }
    path.to_path_buf()
}

#[tauri::command]
fn save_project(path: String, data: String) -> Result<String, String> {
    let p = find_available_path(std::path::Path::new(&path));
    std::fs::write(&p, data).map_err(|e| e.to_string())?;
    Ok(p.to_string_lossy().to_string())
}

#[tauri::command]
fn load_project(path: String) -> Result<String, String> {
    std::fs::read_to_string(&path).map_err(|e| e.to_string())
}

#[tauri::command]
fn save_image(path: String, data: Vec<u8>) -> Result<String, String> {
    let p = find_available_path(std::path::Path::new(&path));
    std::fs::write(&p, data).map_err(|e| e.to_string())?;
    Ok(p.to_string_lossy().to_string())
}

/// フォントディレクトリを再帰スキャンして family 名候補を収集する。
fn scan_fonts(dir: &str, out: &mut std::collections::BTreeSet<String>) {
    let Ok(rd) = std::fs::read_dir(dir) else {
        return;
    };
    for entry in rd.flatten() {
        let path = entry.path();
        if path.is_dir() {
            scan_fonts(&path.to_string_lossy(), out);
            continue;
        }
        let ext = path
            .extension()
            .map(|e| e.to_string_lossy().to_lowercase())
            .unwrap_or_default();
        if !matches!(ext.as_str(), "ttf" | "otf" | "ttc") {
            continue;
        }
        if let Some(stem) = path.file_stem() {
            let name = clean_font_stem(&stem.to_string_lossy());
            if name.len() >= 2 {
                out.insert(name);
            }
        }
    }
}

/// フォントファイル名からスタイル接尾辞を取り除いて family 名を近似する。
fn clean_font_stem(stem: &str) -> String {
    // ハイフン区切りの場合は最初のトークンを family 名とみなす
    if let Some(idx) = stem.find('-') {
        let head = stem[..idx].trim();
        if !head.is_empty() {
            return head.to_string();
        }
    }
    // 旧 Windows フォントの短縮接尾辞（bd=Bold, bi=BoldItalic 等）を除去
    let short_suffixes = ["bd", "bi", "it", "z"];
    let mut s = stem.to_string();
    for suf in &short_suffixes {
        if s.len() > suf.len() + 3 && s.to_lowercase().ends_with(suf) {
            s = s[..s.len() - suf.len()].to_string();
            break;
        }
    }
    s.trim().to_string()
}

#[tauri::command]
fn get_system_fonts() -> Vec<String> {
    let mut names: std::collections::BTreeSet<String> = std::collections::BTreeSet::new();
    // 汎用ファミリは常に追加
    for f in &["serif", "sans-serif", "monospace", "cursive", "fantasy", "system-ui"] {
        names.insert(f.to_string());
    }

    #[cfg(target_os = "windows")]
    {
        scan_fonts(r"C:\Windows\Fonts", &mut names);
        if let Ok(up) = std::env::var("USERPROFILE") {
            scan_fonts(
                &format!(r"{}\AppData\Local\Microsoft\Windows\Fonts", up),
                &mut names,
            );
        }
    }
    #[cfg(target_os = "macos")]
    {
        scan_fonts("/System/Library/Fonts", &mut names);
        scan_fonts("/Library/Fonts", &mut names);
        if let Ok(home) = std::env::var("HOME") {
            scan_fonts(&format!("{}/Library/Fonts", home), &mut names);
        }
    }
    #[cfg(not(any(target_os = "windows", target_os = "macos")))]
    {
        scan_fonts("/usr/share/fonts", &mut names);
        scan_fonts("/usr/local/share/fonts", &mut names);
        if let Ok(home) = std::env::var("HOME") {
            scan_fonts(&format!("{}/.local/share/fonts", home), &mut names);
        }
    }

    names.into_iter().collect()
}

#[cfg_attr(mobile, tauri::mobile_entry_point)]
pub fn run() {
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .plugin(tauri_plugin_fs::init())
        .invoke_handler(tauri::generate_handler![
            save_project,
            load_project,
            save_image,
            get_system_fonts,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
