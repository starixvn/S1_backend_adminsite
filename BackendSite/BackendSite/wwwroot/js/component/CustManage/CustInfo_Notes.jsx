export class Notes extends React.Component {
    constructor(props) {
        super(props);
        this.state = { note: "", isEdit: {}, newNote: null };
    }

    componentDidMount() {
        if (this.props.Notes == null) {
            this.props.GetCustNotes();
        }
    }

    handleSubmit(event) {
        event.preventDefault();
        this.props.AddCustNote(this.state.note);
    }

    onChange(event) {
        let obj = this.state;
        obj[event.target.name] = event.target.value;
        this.setState(obj);
    }

    handleEdit(index) {
        let obj = this.state.isEdit;
        Object.keys(obj).map((val) => {
            obj[val] = (val == index) ? obj[val] === true ? false : true : false;
        })
        this.setState({ newNote: null })
        this.setState(obj);
    };

    handleUpdate(index, type) {
        let obj = this.state.isEdit;
        this.props.SetCustNote(index, this.state.newNote, type);
        (type === 1) ? obj[index] = obj[index] === true ? false : true : null;
        this.setState({ newNote: null })
        this.setState(obj);
    };

    render() {
        let notes = this.props.Notes;
        let notesList = null;
        if (notes != null) {
            notesList =
                <ul>
                    {notes.map((note) => {
                        this.state.isEdit[note.id] = this.state.isEdit[note.id] === true;
                        return (
                            <li key={note.id}>
                                {this.state.isEdit[note.id] && subFuncKeys.includes('ca_noteEdit') ? (
                                    <input className="content-edit" type="text" onChange={this.onChange.bind(this)} name="newNote" />

                                ) : (
                                        <p className="content" style={{ overflowWrap: "anywhere" }}>{note.note}</p>
                                    )}
                                <p className="author">
                                    <span className="user" style={{ paddingRight: "10px" }}>{note.operator}</span>
                                    <span className="date">{(note.updateTime) ? moment(note.updateTime).format('l LTS (Z)') : moment(note.createTime).format('l LTS (Z)')}</span>
                                </p>
                                {

                                    subFuncKeys.includes('ca_noteEdit') ? (
                                        this.state.isEdit[note.id] ? (
                                            <p id="fonts-wrapper">
                                                <i className="fa fa-check-square" onClick={this.handleUpdate.bind(this, note.id, 1)}></i>
                                                <i className="fa fa-window-close" onClick={this.handleEdit.bind(this, note.id)}></i>
                                            </p>
                                        ) : (
                                                <p id="fonts-wrapper">
                                                    <i className="fa fa-edit" onClick={this.handleEdit.bind(this, note.id)}></i>
                                                    <i className="fa fa-trash-alt" onClick={this.handleUpdate.bind(this, note.id, 2)}></i>
                                                </p >
                                            )
                                    ) : null
                                }
                            </li>
                        )
                    })}
                </ul >;
        }
        return (
            <div>
                <div className="wrapper-note">
                    <div id="add-notes">
                        <form id="add" className="form-add" onSubmit={this.handleSubmit.bind(this)}>
                            <input className="add-note-field" type="text" onChange={this.onChange.bind(this)} placeholder="Add a note..." name="note" value={this.state.note} />
                            <input className="btn btn-add" type="submit" value="Add" />
                        </form>
                    </div>
                    <div className="note-list">
                        {notesList}
                    </div>
                </div>
            </div >
        )
    }
}
