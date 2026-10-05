import 'package:flutter/foundation.dart';

import '../../../core/api/api_client.dart';
import '../../../core/config/app_config.dart';
import '../data/cv_file_picker.dart';
import '../data/cv_repository.dart';
import '../models/cv_profile.dart';

class CvSelectionViewModel extends ChangeNotifier {
  CvSelectionViewModel(this._filePicker, this._repository);

  final CvFilePicker _filePicker;
  final CvDataSource _repository;
  SelectedCv? _selectedCv;
  CvProfile? _profile;
  bool _isSelecting = false;
  bool _isLoading = false;
  bool _isUploading = false;
  bool _isSaving = false;
  String? _errorMessage;
  String? _successMessage;

  SelectedCv? get selectedCv => _selectedCv;
  CvProfile? get profile => _profile;
  bool get isSelecting => _isSelecting;
  bool get isLoading => _isLoading;
  bool get isUploading => _isUploading;
  bool get isSaving => _isSaving;
  String? get errorMessage => _errorMessage;
  String? get successMessage => _successMessage;

  Future<void> load() async {
    if (_isLoading) return;
    _isLoading = true;
    _errorMessage = null;
    notifyListeners();
    try {
      _profile = await _repository.get();
    } on ApiException catch (error) {
      _errorMessage = error.message;
    } on Object {
      _errorMessage = 'Your CV profile could not be loaded.';
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> selectCv() async {
    if (_isSelecting) return;
    _isSelecting = true;
    _errorMessage = null;
    notifyListeners();
    try {
      final selected = await _filePicker.pick();
      if (selected == null) return;
      final extension = selected.name.split('.').last.toLowerCase();
      if (!const {'pdf', 'docx'}.contains(extension)) {
        _errorMessage = 'Choose a PDF or DOCX file.';
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

  Future<bool> upload() async {
    final selected = _selectedCv;
    if (selected == null || _isUploading) return false;
    _isUploading = true;
    _errorMessage = null;
    _successMessage = null;
    notifyListeners();
    try {
      _profile = await _repository.upload(selected);
      _selectedCv = null;
      _successMessage = 'Text extracted. Review and confirm every field.';
      return true;
    } on ApiException catch (error) {
      _errorMessage = error.message;
      return false;
    } on Object {
      _errorMessage = 'Your CV could not be uploaded.';
      return false;
    } finally {
      _isUploading = false;
      notifyListeners();
    }
  }

  Future<bool> confirm(CvProfileUpdate update) async {
    _isSaving = true;
    _errorMessage = null;
    _successMessage = null;
    notifyListeners();
    try {
      _profile = await _repository.confirm(update);
      _successMessage = 'CV profile confirmed. Recommendations are ready.';
      return true;
    } on ApiException catch (error) {
      _errorMessage = error.message;
      return false;
    } finally {
      _isSaving = false;
      notifyListeners();
    }
  }

  Future<void> deleteCv() async {
    try {
      await _repository.delete();
      _profile = null;
      _selectedCv = null;
      _successMessage = 'Your CV and extracted profile were deleted.';
      _errorMessage = null;
    } on ApiException catch (error) {
      _errorMessage = error.message;
    }
    notifyListeners();
  }

  void removeSelection() {
    _selectedCv = null;
    _errorMessage = null;
    notifyListeners();
  }
}
