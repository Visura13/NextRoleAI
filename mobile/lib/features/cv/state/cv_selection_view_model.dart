import 'package:flutter/foundation.dart';

import '../../../core/config/app_config.dart';
import '../data/cv_file_picker.dart';

class CvSelectionViewModel extends ChangeNotifier {
  CvSelectionViewModel(this._filePicker);

  final CvFilePicker _filePicker;
  SelectedCv? _selectedCv;
  bool _isSelecting = false;
  String? _errorMessage;

  SelectedCv? get selectedCv => _selectedCv;
  bool get isSelecting => _isSelecting;
  String? get errorMessage => _errorMessage;

  Future<void> selectCv() async {
    if (_isSelecting) return;
    _isSelecting = true;
    _errorMessage = null;
    notifyListeners();
    try {
      final selected = await _filePicker.pick();
      if (selected == null) return;
      final extension = selected.name.split('.').last.toLowerCase();
      if (!const {'pdf', 'doc', 'docx'}.contains(extension)) {
        _errorMessage = 'Choose a PDF, DOC, or DOCX file.';
        return;
      }
      if (selected.sizeInBytes <= 0 ||
          selected.sizeInBytes > AppConfig.maximumCvBytes) {
        _errorMessage = 'The CV must be smaller than 5 MB.';
        return;
      }
      _selectedCv = selected;
    } on Object {
      _errorMessage = 'The document picker could not be opened.';
    } finally {
      _isSelecting = false;
      notifyListeners();
    }
  }

  void removeSelection() {
    _selectedCv = null;
    _errorMessage = null;
    notifyListeners();
  }
}
