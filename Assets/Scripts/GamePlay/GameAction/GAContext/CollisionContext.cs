using cfg;

public class CollisionContext{
    public readonly BoardEntity initiator;
    public readonly BoardEntity receiver;
    public CollisionContext(BoardEntity initiator, BoardEntity receiver){
        this.initiator = initiator;
        this.receiver = receiver;
    }
}