import { Editor } from "react-draft-wysiwyg";
import "react-draft-wysiwyg/dist/react-draft-wysiwyg.css";
import draftToHtml from "draftjs-to-html";
import htmlToDraft from "html-to-draftjs";
import { EditorState, convertToRaw, ContentState } from "draft-js";
const AddPanel = (props) => {
  const [stateData, updateData] = React.useState({
    title: "",
    content: "",
  });

  const ClickSend = () => {
    props.ClickSend(stateData);
  };

  const onEditorStateChange = (editorState) => {
    let editorStatehtml = draftToHtml(
      convertToRaw(editorState.getCurrentContent())
    );
    updateData({...stateData, content:editorStatehtml});
  };
  let content = EditorState.createEmpty();
  return (
    <form>
      <button
        type="button"
        className="btn btn-back"
        onClick={props.ClickBack}
        style={{ marginBottom: "1rem" }}
      >
        Back
      </button>
      <div style={{ marginBottom: "1rem" }}>
        <p style={{ marginBottom: 0 }} className="label">
          User Prefer Language:
        </p>
        <span>{props.UserLang}</span>
      </div>
      <div className="form-group">
        <label>Subject</label>
        <input
          type="text"
          className="form-control"
          onBlur={(e) => {
            updateData({ ...stateData, title: e.target.value });
          }}
        />
      </div>
      <div>
        <label>Content</label>
          <Editor
            defaultEditorState={content}
            toolbarClassName="toolbar-class"
            wrapperClassName="wrapper-class"
            editorClassName="editor-class"
            onEditorStateChange={onEditorStateChange}
          />
        {/* <textarea className="form-control" rows="8" onBlur={(e)=>{updateData({...stateData, content:e.target.value})}}></textarea> */}
      </div>
      <button type="button" className="btn btn-add" onClick={ClickSend}>
        Send
      </button>
    </form>
  );
};

export default AddPanel;
